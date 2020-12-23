namespace Soap.MessagePipeline
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Models;
    using DataStore.Models.Messages;
    using Soap.Bus;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    

    public static class ContextWithMessageLogEntryExtensions
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
            will be continued when the message is next dequeued, updates using a new datastore instance not in session */
            await context.SaveUnitOfWork();

            Guard.Against(
                context.Message.Headers.GetMessageId() == SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully
                && context.MessageLogEntry.Attempts.Count == 0,
                SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully.ToString());

            await context.DataStore.CommitChanges();
            
            Guard.Against(
                context.Message.Headers.GetMessageId() == SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully
                && context.MessageLogEntry.Attempts.Count == 0,
                SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully.ToString());

            //* messages may be resent but will be thrown out if they have already been sent, importantly they will not change ids
            await context.Bus.CommitChanges();

            Guard.Against(
                context.Message.Headers.GetMessageId()
                == SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully
                && context.MessageLogEntry.Attempts.Count == 0,
                SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully.ToString());

            await context.MessageLogEntry.CompleteUnitOfWork(context.DataStore.DocumentRepository.ConnectionSettings);

            /* any other arbitrary calls made e.g. to 3rd party API etc. these will not be persisted but they should be arrange such that they are handled on isolated calls
            hence the guard above so you cannot queue durable and non-durable changes in the same unit of work. 
            TODO: the call to getqueuedchanges would pickup datastore and bus ops if they hadn't been committed above
            maybe it should filter them out? can't see how without bugs in datastore or bus's commitchanges that would matter
            but if there were such bugs this might mask them? */
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
                    var u when IsEmpty(u) => UnitOfWork.State.New, //* hasn't been saved yet, msg not processed yet
                    _ => await AttemptCompletion(records, context)
                };
            }

            static async Task<UnitOfWork.State> AttemptCompletion(
                List<UnitOfWorkExtensions.Record> records,
                ContextWithMessageLogEntry context)
            {
                var messageLogEntry = context.MessageLogEntry;

                if (!messageLogEntry.UnitOfWork.OptimisticConcurrency) return await CompleteDataAndMessages(records, context);

                return records switch //* warning: the order of the switches does matter
                {
                    var r when PartiallyCompletedDataButCannotFinish(r) => await RollbackRemaining(
                                                                               r,
                                                                               context.DataStore.DocumentRepository,
                                                                               context),
                    var r when NotStartedOrPartiallyCompletedDataAndCanFinish(r) => await CompleteDataAndMessages(r, context),
                    var r when HasNotStartedOrWasRolledBackButCannotFinish(r, messageLogEntry) => UnitOfWork.State.AllRolledBack,
                    var r when CompletedDataButNotMarkedAsCompleted(r, messageLogEntry) => await CompleteMessages(context),
                    _ => throw new DomainException(
                             "Unaccounted for case in handling failed unit of work" + $" {messageLogEntry.id}")
                };

                static async Task<UnitOfWork.State> RollbackRemaining(
                    List<UnitOfWorkExtensions.Record> records,
                    IDocumentRepository documentRepository,
                    ContextWithMessageLogEntry context)
                {
                    //* arrange in the manner so that records most likely to be affected by other Uows are rolled back first
                    var recordsToRollback = records.Where(x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed)
                                                   .OrderBy(x => x.Superseded == false) //* if its already superseded no rush
                                                   .ThenBy(
                                                       x => x.UowItem.OperationType
                                                            == DataStoreUnitOfWorkItem.OperationTypes
                                                                                      .Create) //* probably biggest risk as we have to remove them
                                                   .ThenBy(
                                                       x => x.UowItem.OperationType
                                                            == DataStoreUnitOfWorkItem.OperationTypes
                                                                                      .Update); //* no one can really delete a deleted record (save edge cases eg. where another uow rolling back resurrects it)

                    foreach (var record in recordsToRollback)
                    {
                        Guard.Against(
                            context.Message.Headers.GetMessageId() == SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry
                            && context.MessageLogEntry.Attempts.Count == 1
                            && record.UowItem.ObjectId == Guid.Parse("715fb00d-f856-42b9-822e-fc0510c6fab5"),
                            SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry.ToString());

                        if (!record.Superseded)
                        {
                            await record.UowItem.Rollback(Delete, Create, ResetTo);
                        }
                    }

                    return UnitOfWork.State.AllRolledBack;

                    Task Delete<T>(T arg) where T : IAggregate => documentRepository.DeleteAsync(arg);

                    Task Create<T>(T arg) where T : IAggregate => documentRepository.CreateAsync(arg);

                    async Task ResetTo<T>(T arg) where T : IAggregate
                    {
                        arg.Etag = null; //* disable concurrency checks
                        var itemExists = await documentRepository.Exists(arg);
                        if (itemExists)
                        {
                            await documentRepository.UpdateAsync(arg);
                        }
                    }
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
                    await SendAnyUnsentMessages(context.MessageLogEntry.UnitOfWork, context.Bus.BusClient, context.DataStore);
                    await context.MessageLogEntry.CompleteUnitOfWork(context.DataStore.DocumentRepository.ConnectionSettings);
                    return UnitOfWork.State.AllComplete;
                }

                //* failed after rollback was done or before anything was committed (e.g. first item failed concurrency check)
                static bool NotStartedOrPartiallyCompletedDataAndCanFinish(List<UnitOfWorkExtensions.Record> records)
                {
                    var result = records.Any(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack
                             && !ThereAreRecordsThatCannotBeCompleted(records));
                    return result;
                }

                /* a rollback was completed or we failed after committing the unit of work but before committing the first item
                (e.g. first item failed concurrency check) 
                It is possible that the server crashes or something in the aforementioned
                window and then nothing has been committed but could be however that should be caught
                by the previous case NotStartedOrPartiallyCompletedDataAndCanFinish */
                static bool HasNotStartedOrWasRolledBackButCannotFinish(
                    List<UnitOfWorkExtensions.Record> records,
                    MessageLogEntry messageLogEntry)
                {
                    var result = records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack);
                    return result;
                }

                /* could be from messages or just the complete flag itself, in any case the complete
                 method can be called and it will skip over the completed methods */
                static bool CompletedDataButNotMarkedAsCompleted(
                    List<UnitOfWorkExtensions.Record> records,
                    MessageLogEntry messageLogEntry)
                {
                    var result = records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed
                             && messageLogEntry.ProcessingComplete == false);
                    return result;
                }

                static bool PartiallyCompletedDataButCannotFinish(List<UnitOfWorkExtensions.Record> records)
                {
                    var result = ThereAreRecordsThatCannotBeCompleted(records) && records.Any(
                                     x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed);
                    return result;
                }

                static bool ThereAreRecordsThatCannotBeCompleted(List<UnitOfWorkExtensions.Record> records)
                {
                    var result = records.Any(
                        r => r.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack && r.Superseded);
                    return result;
                }
            }

            static bool IsEmpty(UnitOfWork u)
            {
                var result = !u.BusCommandMessages.Any() && !u.BusEventMessages.Any() && !u.DataStoreUpdateOperations.Any()
                             && !u.DataStoreDeleteOperations.Any() && !u.DataStoreCreateOperations.Any();
                return result;
            }

            static async Task SaveUnsavedData(List<UnitOfWorkExtensions.Record> records, IDataStore dataStore)
            {
                {
                    /* use order most likely to cause an eTag violation so that we minimize the number of items
                     needing to be rolled back. 
                     Take updates first whose records probably existed for a while and prioritize those that are modified recently.
                     Deletes dont cause eTag violations and neither do creates so doesn't really matter there
                     but we'll take items being deleted whose model was modified recently as a sign of activity on that 
                     aggregate and take those first
                    */

                    var incompleteRecords = records
                                            .Where(
                                                x => x.State == DataStoreUnitOfWorkItemExtensions
                                                                .RecordState.NotCommittedOrRolledBack)
                                            .OrderBy(
                                                x => x.UowItem.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update)
                                            .ThenByDescending(
                                                x => x.UowItem.BeforeModel?.Deserialise<IAggregate>()
                                                      .ModifiedAsMillisecondsEpochTime)
                                            .ToList();

                    foreach (var incompleteRecord in incompleteRecords) await SaveRecord(incompleteRecord, dataStore);
                }

                static async Task SaveRecord(UnitOfWorkExtensions.Record r, IDataStore dataStore)
                {
                    
                    /* important to understand that you want to avoid any change whatsoever to the aggregate
                     on retries once it has become part of the unit of work which is why we call the underlying 
                     documentrepository then the datastore */
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

            static async Task SendAnyUnsentMessages(UnitOfWork unitOfWork, IBusClient busClient, IDataStore dataStore)
            {
                /* cannot rollback messages, forward only,
                it's not the same risk as data though since there are no concurrency issues
                it's really only infrastructure problems that would stop you here 
                
                also you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
                which is why we call the underlying busclient rather than the bus */
                var incompleteCommands =
                    await unitOfWork.BusCommandMessages.WhereAsync(async x => !await x.IsComplete(dataStore));
                var commands = incompleteCommands.Select(x => x.Deserialise<ApiCommand>()).ToList();
                
                commands.ForEach(async i => await busClient.Send(i));

                var incompleteEvents = await unitOfWork.BusEventMessages.WhereAsync(async x => !await x.IsComplete(dataStore));
                var events = incompleteEvents.Select(x => new { Event = x.Deserialise<ApiEvent>(), Visibility = x.EventVisibility }).ToList();
                events.ForEach(async x => await busClient.Publish(x.Event, x.Visibility));
                
            }

            static async Task<List<UnitOfWorkExtensions.Record>> WaitForAllRecords(
                UnitOfWork unitOfWork,
                IDataStore dataStore)
            {
                var records = new List<UnitOfWorkExtensions.Record>();
                await foreach (var item in unitOfWork.DataStoreOperationState(dataStore)) records.Add(item);

                return records;
            }
        }

        internal static async Task TakeFailureActions(
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
                    await SendFinalFailureMessages();
                }

                await AddThisFailureToTheMessageLog();

                /* commit just the failure entry and possibly finalfailuremessage,
             don't use context.CommitChanges as you don't want to save the unit of work here 
             the uow is already cleared when the message retries you don't want it thinking the uow
             is the failed message log entry saving that and then exiting. it also makes testing failures
             in context.commitchanges easier since that method is not used here as well creating a loop
             */
                
                await context.DataStore.CommitChanges();
                await context.Bus.CommitChanges();
            }

            catch (Exception e)
            {
                throw new ApplicationException(
                    $"Error logging failed message with id {context.Message.Headers.GetMessageId()} to DataStore",
                    e);
            }

            async Task AddThisFailureToTheMessageLog()
            {
                //- in-place update, unlikely to be used, better safe than sorry
                var logEntry = context.MessageLogEntry;
                logEntry.AddFailedAttempt(exceptionInfo);

                await context.DataStore.Update(logEntry, o => o.DisableOptimisticConcurrency()); //etag has changed
            }

            bool TheMessageWeAreProcessingIsAMaxFailNotificationMessage() =>
                //- avoid infinite loop
                context.Message.GetType().InheritsOrImplements(typeof(MessageFailedAllRetries));

            bool ThisFailureIsTheFinalFailure() =>
                //- remember that total attempts is initial message + retries
                context.MessageLogEntry.Attempts.Count == context.Bus.MaximumNumberOfRetries;

            async Task SendFinalFailureMessages()
            {
                var json = context.Message.ToJson(SerialiserIds.ApiBusMessage);
                
                var instanceOfMessageFailedAllRetries = new MessageFailedAllRetries()
                {
                    SerialiserId = SerialiserIds.ApiBusMessage.Key,
                    TypeName = context.Message.GetType().ToShortAssemblyTypeName(),  //* you don't want the assembly version etc since that could break deserialisation
                    SerialisedMessage = json
                };

                await context.Bus.Send(instanceOfMessageFailedAllRetries);

                var exception = exceptionInfo.ExceptionThrownToContext();
                
                var toWsClients = new E001v1_MessageFailed()
                {
                    E001_ErrorMessage = exception.Message,
                    E001_ErrorCodes = exception.KnownErrorCodes,
                    E001_MessageId = context.Message.Headers.GetMessageId(),
                    E001_MessageTypeName = context.Message.GetType().ToShortAssemblyTypeName(),
                    E001_StatefulProcessId = context.Message.Headers.GetStatefulProcessId()
                };
                    
                await context.Bus.Publish(toWsClients, context.Message, new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
            }
        }

        internal static void SerilogFailure(this ContextWithMessageLogEntry context, FormattedExceptionInfo exceptionInfo)
        {
            var message = context.Message;
            var meta = context.MessageLogEntry.MessageMeta;

            var serilogEntry = new FailedAttempt(exceptionInfo)
            {
                SapiReceivedAt = meta?.ReceivedAt.DateTime,
                SapiCompletedAt = DateTime.UtcNow,
                UserName = meta?.RequestedBy?.UserName,
                MessageId = message.Headers.GetMessageId(),
                Schema = message.GetType().FullName,
                Message = message,
                IsCommand = message is ApiCommand,
                IsEvent = message is ApiEvent,
                ProfilingData = meta != null ? CreateProfilingData(meta, context) : null,
                EnvironmentName = context.AppConfig.Environment.Value,
                ApplicationName = context.AppConfig.AppFriendlyName
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
                MessageId = message.Headers.GetMessageId(),
                Schema = message.Headers.GetSchema(),
                Message = message,
                IsCommand = message is ApiCommand,
                IsEvent = message is ApiEvent,
                ProfilingData = CreateProfilingData(meta, context),
                EnvironmentName = context.AppConfig.Environment.Value,
                ApplicationName = context.AppConfig.AppFriendlyName
            };

            context.Logger.Information("Message: {@message}", serilogEntry);
        }

        internal static ContextWithMessageLogEntry Upgrade(this ContextWithMessage current, MessageLogEntry messageLogEntry) =>
            new ContextWithMessageLogEntry(messageLogEntry, current);

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
                                Name = stateOperation.GetType().ToTypeNameString(),
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
                                Name = stateOperation.GetType().ToTypeNameString()
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

                bool IsInvalidTimestampValue(long? timestamp) => timestamp.HasValue == false || timestamp <= 0;

                bool IsStopTimestampBeforeStartTimestamp(long start, long? stop) => stop < start;

                bool IsStartTimestampBeforePrevoiusStopTimestamp(long previousStop, long currentStart) =>
                    currentStart < previousStop;
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
            var result = x is IQueuedDataStoreWriteOperation || x is IQueuedBusOperation;

            return result;
        }

        private static Task SaveUnitOfWork(this ContextWithMessageLogEntry context)
        {
            Guard.Against(
                context.Message.Headers.GetMessageId() == SpecialIds.MessageDiesWhileSavingUnitOfWork,
                SpecialIds.MessageDiesWhileSavingUnitOfWork.ToString());
            
            var u = context.MessageLogEntry.UnitOfWork;

            foreach (var queuedStateChange in context.GetQueuedChanges())
                if (IsDurableChange(queuedStateChange))
                {
                    var soapUnitOfWorkId = Guid.Parse(context.DataStore.DataStoreOptions.UnitOfWorkId);

                    switch (queuedStateChange)
                    {
                        case IQueuedBusOperation b1 when b1.GetType().InheritsOrImplements(typeof(QueuedCommandToSend)):
                            u.BusCommandMessages.Add(new BusMessageUnitOfWorkItem(((QueuedCommandToSend)b1).CommandToSend, null));
                            break;

                        case IQueuedBusOperation b1 when b1.GetType().InheritsOrImplements(typeof(QueuedEventToPublish)):
                            u.BusEventMessages.Add(new BusMessageUnitOfWorkItem(((QueuedEventToPublish)b1).EventToPublish, ((QueuedEventToPublish)b1).EventVisibility));
                            break;

                        case IQueuedDataStoreWriteOperation d1 when d1.GetType().InheritsOrImplements(typeof(QueuedCreateOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Create));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.GetType().InheritsOrImplements(typeof(QueuedHardDeleteOperation<>)):
                            u.DataStoreDeleteOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.GetType().InheritsOrImplements(typeof(QueuedUpdateOperation<>)):
                            u.DataStoreUpdateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Update));
                            break;
                        default:
                            context.Logger.Debug("Could not save durable item to unit of work.");
                            break;
                    }
                }

            /* updates using another datastore instance not in session */
            return context.MessageLogEntry.UpdateUnitOfWork(u, context.DataStore.DocumentRepository.ConnectionSettings);
            /* from this point on we can crash, throw, lose power, it won't matter all
        will be continued when the message is next dequeued*/
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
