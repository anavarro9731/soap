namespace Soap.MessagePipeline.Context
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.Messages;
    using Soap.Bus;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.UnitOfWork;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Models;

    public class ContextWithMessageLogEntry : ContextWithMessage
    {
        public ContextWithMessageLogEntry(MessageLogEntry messageLogEntry, ContextWithMessage current)
            : base(current)
        {
            MessageLogEntry = messageLogEntry;
        }

        public MessageLogEntry MessageLogEntry { get; }
    }

    public static class ContextAfterMessageLogEntryObtainedExtensions
    {
        public static async Task CommitChanges(this ContextWithMessageLogEntry context)
        {
            Guard.Against(
                context.GetQueuedChanges().Count > 1 && context.GetQueuedChanges().Any(x => !IsDurableChange(x)),
                "You cannot queue durable and non-durable changes in the same unit of work. If a unit of work contains a non-durable"
                + " change it must be the only change in the unit of work. i.e. you must send non-durable changes to their own unit-of-work"
                + " using a new message. This is ensure the UoW can be persisted in case of failure. All other I/O bound ops must be pushed"
                + " to the perimeter");

            /* from this point on we can crash, throw, lose power, it won't matter all
            will be continued when the message is next dequeued*/
            await context.SaveUnitOfWork();

            await context.DataStore.CommitChanges();
            await context.Bus.CommitChanges();
            await context.MessageLogEntry.CompleteUnitOfWork(context.DataStore.DocumentRepository.ConnectionSettings);

            /* any other arbitrary calls made e.g. to 3rd party API etc. */
            foreach (var queuedStateChange in context.GetQueuedChanges())
            {
                await queuedStateChange.CommitClosure();
                queuedStateChange.Committed = true;
            }
        }

        public static void QueueChange(this ContextWithMessageLogEntry context, IQueuedStateChange queuedStateChange)
        {
            /* any other arbitrary calls made e.g. to 3rd party API etc. */
            context.MessageAggregator.Collect(queuedStateChange);
        }

        public static Task SaveUnitOfWork(this ContextWithMessageLogEntry context)
        {
            var u = context.MessageLogEntry.UnitOfWork;

            foreach (var queuedStateChange in context.GetQueuedChanges())
                if (IsDurableChange(queuedStateChange))
                {
                    var soapUnitOfWorkId = context.MessageLogEntry.id;

                    switch (queuedStateChange)
                    {
                        case QueuedApiCommand c:
                            u.BusCommandMessages.Add(new BusMessageUnitOfWorkItem(c.Command));
                            break;

                        case QueuedApiEvent e:
                            u.BusEventMessages.Add(new BusMessageUnitOfWorkItem(e.Event));
                            break;

                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedCreateOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Create));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedHardDeleteOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedUpdateOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Update));
                            break;
                    }
                }

            return context.MessageLogEntry.UpdateUnitOfWork(u, context.DataStore.DocumentRepository.ConnectionSettings);
            /* from this point on we can crash, throw, lose power, it won't matter all
            will be continued when the message is next dequeued*/
        }

        internal static async Task<UnitOfWork.State> AttemptToFinishAnUnfinishedUnitOfWork(
            this ContextWithMessageLogEntry context)
        {
            {
                /* check the ds UoW's look ahead first to see if there are potential conflicts
                 if there are then we can assume that is why we failed last time and we should rollback any remaining items
                 starting with the creates and updates since they are the ones that other people could have seen
                 and finally returning AllRolledBack */

                //- don't recalculate these expensive ops
                var records = await WaitForAllRecords(context.MessageLogEntry.UnitOfWork, context.DataStore);

                return context.MessageLogEntry.UnitOfWork switch
                {
                    var u when IsEmpty(u) => UnitOfWork.State.New,
                    _ => await AttemptCompletion(records, context)
                };
            }

            static async Task<UnitOfWork.State> AttemptCompletion(
                List<UnitOfWorkExtensions.Record> records,
                ContextWithMessageLogEntry context)
            {
                var messageLogEntry = context.MessageLogEntry;

                if (!messageLogEntry.UnitOfWork.OptimisticConcurrency) return await CompleteDataAndMessages(records, context);

                return records switch
                {
                    var r when HasNotStartedDataButCannotFinish(r, messageLogEntry) => UnitOfWork.State.AllRolledBack,
                    var r when PartiallyCompletedDataButCannotFinish(r) => await RollbackRemaining(r),
                    var r when PartiallyCompletedDataAndCanFinish(r) => await CompleteDataAndMessages(r, context),
                    var r when CompletedDataButNotMarkedAsCompleted(r, messageLogEntry) => await CompleteMessages(context),
                    _ => throw new DomainException(
                             "Unaccounted for case in handling failed unit of work" + $" {messageLogEntry.id}")
                };

                static async Task<UnitOfWork.State> RollbackRemaining(List<UnitOfWorkExtensions.Record> records)
                {
                    foreach (var record in records.Where(x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed))
                        await record.UowItem.Rollback(null, null, null);
                    return UnitOfWork.State.AllRolledBack;
                }

                static async Task<UnitOfWork.State> CompleteDataAndMessages(
                    List<UnitOfWorkExtensions.Record> records,
                    ContextWithMessageLogEntry context)
                {
                    await SaveUnsavedData(records, context.DataStore);
                    return await CompleteMessages(context);
                }

                static async Task<UnitOfWork.State> CompleteMessages(ContextWithMessageLogEntry context)
                {
                    await SendAnyUnsentMessages(context.MessageLogEntry.UnitOfWork, context.Bus, context.DataStore);
                    await context.MessageLogEntry.CompleteUnitOfWork(context.DataStore.DocumentRepository.ConnectionSettings);
                    return UnitOfWork.State.AllComplete;
                }

                //* failed after rollback was done or before anything was committed (e.g. first item failed concurrency check)
                static bool PartiallyCompletedDataAndCanFinish(List<UnitOfWorkExtensions.Record> records)
                {
                    return records.Any(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack
                             && !ThereAreRecordsThatCannotBeCompleted(records));
                }

                //* failed after rollback was done or before anything was committed (e.g. first item failed concurrency check)
                static bool HasNotStartedDataButCannotFinish(
                    List<UnitOfWorkExtensions.Record> records,
                    MessageLogEntry messageLogEntry)
                {
                    return records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack
                             && messageLogEntry.ProcessingComplete == false);
                }

                /* could be from messages or just the complete flag itself, in any case the complete
                method can be called and it will skip over the completed methods */
                static bool CompletedDataButNotMarkedAsCompleted(
                    List<UnitOfWorkExtensions.Record> records,
                    MessageLogEntry messageLogEntry)
                {
                    return records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed
                             && messageLogEntry.ProcessingComplete == false);
                }

                static bool PartiallyCompletedDataButCannotFinish(List<UnitOfWorkExtensions.Record> records)
                {
                    return ThereAreRecordsThatCannotBeCompleted(records) && records.Any(
                               x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed);
                }

                static bool ThereAreRecordsThatCannotBeCompleted(List<UnitOfWorkExtensions.Record> records)
                {
                    return records.Any(
                        r => r.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack && r.Superseded);
                }
            }

            static bool IsEmpty(UnitOfWork u)
            {
                return !u.BusCommandMessages.Any() && !u.BusEventMessages.Any() && !u.DataStoreUpdateOperations.Any()
                       && !u.DataStoreDeleteOperations.Any() && !u.DataStoreCreateOperations.Any();
            }

            static async Task SaveUnsavedData(List<UnitOfWorkExtensions.Record> records, IDataStore dataStore)
            {
                {
                    /* use order least vulnerable to rollbacks,
                 take updates which and prioritize those that are modified recently 
                 then take creates, because deletes are always allowed through both physically
                 and logically (I don't think we care about race condition when deleting) and if a create
                 fails it means less to rollback if the deletes have not been done yet*/

                    var incompleteRecords = records
                                            .Where(
                                                x => x.State == DataStoreUnitOfWorkItemExtensions
                                                                .RecordState.NotCommittedOrRolledBack)
                                            .OrderBy(
                                                x => x.UowItem.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update)
                                            .ThenBy(x => x.UowItem.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create)
                                            .ThenByDescending(
                                                x => x.UowItem.BeforeModel?.Deserialise<IAggregate>()
                                                      .ModifiedAsMillisecondsEpochTime)
                                            .ToList();

                    foreach (var incompleteRecord in incompleteRecords) await SaveRecord(incompleteRecord, dataStore);
                }

                static async Task SaveRecord(UnitOfWorkExtensions.Record r, IDataStore dataStore)
                {
                    var before = r.UowItem.BeforeModel?.Deserialise<IAggregate>();
                    var after = r.UowItem.AfterModel?.Deserialise<IAggregate>();

                    switch (r.UowItem.OperationType)
                    {
                        case DataStoreUnitOfWorkItem.OperationTypes.Update:
                            await dataStore.DocumentRepository.UpdateAsync(after, nameof(AttemptToFinishAnUnfinishedUnitOfWork));
                            break;
                        case DataStoreUnitOfWorkItem.OperationTypes.Create:
                            await dataStore.DocumentRepository.CreateAsync(after, nameof(AttemptToFinishAnUnfinishedUnitOfWork));
                            break;
                        case DataStoreUnitOfWorkItem.OperationTypes.HardDelete:
                            await dataStore.DocumentRepository.DeleteAsync(before, nameof(AttemptToFinishAnUnfinishedUnitOfWork));
                            break;
                    }
                }
            }

            static async Task SendAnyUnsentMessages(UnitOfWork unitOfWork, IBus busContext, IDataStore dataStore)
            {
                /* cannot rollback messages, forward only,
            it's not the same risk as data though since there are no concurrency issues
            it's really only infrastructure problems that would stop you here */
                var incompleteCommands =
                    await unitOfWork.BusCommandMessages.WhereAsync(async x => !await x.IsComplete(dataStore));
                incompleteCommands.Select(x => x.Deserialise<ApiCommand>()).ToList().ForEach(busContext.Send);

                var incompleteEvents = await unitOfWork.BusEventMessages.WhereAsync(async x => !await x.IsComplete(dataStore));
                incompleteEvents.Select(x => x.Deserialise<ApiEvent>()).ToList().ForEach(busContext.Publish);
            }

            static async Task<List<UnitOfWorkExtensions.Record>> WaitForAllRecords(UnitOfWork unitOfWork, IDataStore dataStore)
            {
                var records = new List<UnitOfWorkExtensions.Record>();
                await foreach (var item in unitOfWork.DataStoreOperationState(dataStore)) records.Add(item);

                return records;
            }
        }

        internal static async Task MarkFailureInMessageLog(
            this ContextWithMessageLogEntry context,
            FormattedExceptionInfo exceptionInfo)
        {
            /* It is possible we could fail while handling a failed message but before we hit this block
                * in that instance we will fail to record the error and the message's error count will remain the same.
                * The message will be retried again as if it was the first failure. You will need queue-level retry limits
                * to prevent messages being retried indefinitely until the underlying problem is fixed.
                * 
                * It is also possible we could fail after writing this block however that would only result in a different
                * final exception being thrown to the endpoint. This new exception will be logged by Serilog and will not
                * change the behaviour of the system as the endpoint runtime doesn't care what the error thrown to it is
                * only that an error is thrown.
                *
                * In both cases a machine losing power should also not change the ultimate behaviour of the system.
                *
                * If the error happens before the unit-of-work is persisted then the message will be retried and the unit-of-work
                * will be recalculated. If the error occurs after the unit-of-work has been persisted, then the message will be
                * retried and on the retry we try to republish the uow from an aggregate. In the event that all retries fail
                * we could be left with a partially unpublished unit-of-work and a poison message. Logic needs to be in place
                * at the beginning of the execution stack to attempt to unwind the unit of work if possible in this case.
               */

            try
            {
                /* abandon current unit of work, maybe it's persisted maybe not, maybe partially complete, or even fully complete
                in all cases all we care about now is recording the failure, no other I/O bounds ops will occur */
                context.MessageAggregator.Clear();

                if (ThisFailureIsTheFinalFailure() && !TheMessageWeAreProcessingIsAMaxFailNotificationMessage())
                {
                    /* send a message to the bus which tells us this has occured
                     allowing us to handle these cases with compensating logic */
                    SendFinalFailureMessage();
                }

                await AddThisFailureToTheMessageLog();

                await context.CommitChanges(); //* commit just the failure entry
            }

            catch (Exception e)
            {
                throw new Exception($"Error logging failed message {context.Message.MessageId} result to DataStore", e);
            }

            async Task AddThisFailureToTheMessageLog()
            {
                //- in-place update, unlikely to be used, better safe than sorry
                var logEntry = context.MessageLogEntry;
                logEntry.AddFailedAttempt(exceptionInfo);

                await context.DataStore.Update(logEntry);
            }

            bool TheMessageWeAreProcessingIsAMaxFailNotificationMessage()
            {
                //- avoid infinite loop
                return context.Message.GetType().InheritsOrImplements(typeof(MessageFailedAllRetries));
            }

            bool ThisFailureIsTheFinalFailure()
            {
                //- remember that total attempts is initial message + retries
                return context.MessageLogEntry.Attempts.Count == context.AppConfig.NumberOfApiMessageRetries;
            }

            void SendFinalFailureMessage()
            {
                var genericTypeWithParam = typeof(MessageFailedAllRetries<>).MakeGenericType(context.Message.GetType());

                var instanceOfMesageFailedAllRetries =
                    (MessageFailedAllRetries)Activator.CreateInstance(genericTypeWithParam, context.Message.MessageId);

                instanceOfMesageFailedAllRetries.MessageId = Guid.NewGuid();
                instanceOfMesageFailedAllRetries.TimeOfCreationAtOrigin = DateTime.UtcNow;
                instanceOfMesageFailedAllRetries.IdOfMessageThatFailed = context.Message.MessageId;

                if (context.Message is ApiCommand command)
                {
                    instanceOfMesageFailedAllRetries.StatefulProcessIdOfMessageThatFailed = command.StatefulProcessId;
                }

                context.Bus.Send(instanceOfMesageFailedAllRetries);
            }
        }

        internal static void SerilogFailure(
            this ContextWithMessageLogEntry context,
            FormattedExceptionInfo exceptionInfo)
        {
            var message = context.Message;
            var meta = context.MessageLogEntry.MessageMeta;

            var serilogEntry = new FailedAttempt(exceptionInfo)
            {
                SapiReceivedAt = meta?.ReceivedAt.DateTime,
                SapiCompletedAt = DateTime.UtcNow,
                UserName = meta?.RequestedBy?.UserName,
                MessageId = message.MessageId,
                Schema = message.GetType().FullName,
                Message = message,
                IsCommand = message is ApiCommand,
                IsQuery = message is IApiQuery,
                IsEvent = message is ApiEvent,
                ProfilingData = meta != null ? CreateProfilingData(meta, context) : null,
                EnvironmentName = context.AppConfig.EnvironmentName,
                ApplicationName = context.AppConfig.ApplicationName
            };

            context.Logger.Error("Message: {@message}", serilogEntry);
        }

        internal static void SerilogSuccess(this ContextWithMessageLogEntry context)
        {
            var meta = context.MessageLogEntry.MessageMeta;
            var message = context.Message;

            var serilogEntry = new SuccessfulAttempt
            {
                Succeeded = true,
                SapiReceivedAt = meta.ReceivedAt.DateTime,
                SapiCompletedAt = DateTime.UtcNow,
                UserName = meta.RequestedBy?.UserName,
                MessageId = message.MessageId,
                Schema = meta.Schema,
                Message = message,
                IsCommand = message is ApiCommand,
                IsQuery = message is IApiQuery,
                IsEvent = message is ApiEvent,
                ProfilingData = CreateProfilingData(meta, context),
                EnvironmentName = context.AppConfig.EnvironmentName,
                ApplicationName = context.AppConfig.ApplicationName
            };

            context.Logger.Information("Message: {@message}", serilogEntry);
        }

        private static object CreateProfilingData(MessageMeta meta, ContextWithMessageLogEntry ctx)
        {
            {
                var initialTimestamp = meta.ReceivedAt.Ticks;

                var finalTimestamp = StopwatchOps.GetStopwatchTimestamp();

                var previousTimestamp = initialTimestamp;

                var stateOperations = new List<object>();

                foreach (var stateOperation in ctx.MessageAggregator.AllMessages.OfType<IStateOperation>())
                {
                    if (HasValidTimestamps(stateOperation, previousTimestamp) == false)
                    {
                        continue;
                    }

                    stateOperations.Add(
                        new
                        {
                            Duration = StopwatchOps.CalculateLatency(
                                                       previousTimestamp,
                                                       stateOperation.StateOperationStartTimestamp)
                                                   .ToString(),
                            Name = "CPU Operation(s)"
                        });

                    if (stateOperation is IDataStoreOperation dataStoreOperation)
                    {
                        stateOperations.Add(
                            new
                            {
                                Duration = stateOperation.StateOperationDuration.ToString(),
                                Name = stateOperation.GetType().AsTypeNameString(),
                                DataStore = dataStoreOperation.MethodCalled + (dataStoreOperation.TypeName != null ? "<" : "")
                                                                            + dataStoreOperation.TypeName.SubstringAfterLast('.')
                                                                            + (dataStoreOperation.TypeName != null ? ">" : "")
                            });
                    }
                    else
                    {
                        stateOperations.Add(
                            new
                            {
                                Duration = stateOperation.StateOperationDuration.ToString(),
                                Name = stateOperation.GetType().AsTypeNameString()
                            });
                    }

                    previousTimestamp = stateOperation.StateOperationStopTimestamp.GetValueOrDefault();
                }

                stateOperations.Add(
                    new
                    {
                        Duration = StopwatchOps.CalculateLatency(previousTimestamp, finalTimestamp).ToString(),
                        Name = "CPU Operation(s)"
                    });

                var profilingData = new
                {
                    TotalProcessingTime = StopwatchOps.CalculateLatency(initialTimestamp, finalTimestamp),
                    StateOperations = stateOperations
                };

                return profilingData;
            }

            bool HasValidTimestamps(IStateOperation stateOperation, long previousStateOperationStopTimestamp)
            {
                {
                    if (IsInvalidTimestampValue(stateOperation.StateOperationStartTimestamp))
                    {
                        return false;
                    }

                    if (IsInvalidTimestampValue(stateOperation.StateOperationStopTimestamp))
                    {
                        return false;
                    }

                    if (IsStopTimestampBeforeStartTimestamp(
                        stateOperation.StateOperationStartTimestamp,
                        stateOperation.StateOperationStopTimestamp))
                    {
                        return false;
                    }

                    if (IsStartTimestampBeforePrevoiusStopTimestamp(
                        previousStateOperationStopTimestamp,
                        stateOperation.StateOperationStartTimestamp))
                    {
                        return false;
                    }

                    return true;
                }

                bool IsInvalidTimestampValue(long? timestamp)
                {
                    return timestamp.HasValue == false || timestamp <= 0;
                }

                bool IsStopTimestampBeforeStartTimestamp(long start, long? stop)
                {
                    return stop < start;
                }

                bool IsStartTimestampBeforePrevoiusStopTimestamp(long previousStop, long currentStart)
                {
                    return currentStart < previousStop;
                }
            }
        }

        private static List<IQueuedStateChange> GetQueuedChanges(this ContextWithMessageLogEntry context)
        {
            var queuedStateChanges = context.MessageAggregator.AllMessages.OfType<IQueuedStateChange>()
                                            .Where(c => !c.Committed)
                                            .ToList();
            return queuedStateChanges;
        }

        private static bool IsDurableChange(IQueuedStateChange x)
        {
            return x is IQueuedDataStoreWriteOperation || x is IQueuedBusMessage;
        }

        private static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            var results = new ConcurrentQueue<T>();
            var tasks = source.Select(
                async x =>
                    {
                    if (await predicate(x))
                    {
                        results.Enqueue(x);
                    }
                    });
            await Task.WhenAll(tasks);
            return results;
        }
    }
}