namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using CircuitBoard.Permissions;
    using DataStore;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Logging;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.Utility.Functions.Extensions;
    using Soap.If.Utility.Functions.Operations;
    using Soap.Pf.MessageContractsBase.Commands;

    public static class IApiMessageExtensions
    {
        internal static void Authenticate(
            this ApiMessage message,
            IAuthenticateUsers authenticator,
            Action<IIdentityWithPermissions> outIdentity)
        {
            var identity = message.IdentityToken != null ? authenticator.Authenticate(message) : null;
            outIdentity(identity);
        }

        internal static async Task CreateNewLogEntry(this ApiMessage message, Action<MessageLogEntry> outLogEntry)
        {
            try
            {
                MContext.Logger.Debug($"Creating record for msg id {message.MessageId}");

                var newItem = await MContext.DataStore.Create(new MessageLogEntry(message, 
                                  MContext.DataStore.As<DataStore>().DataStoreOptions.OptimisticConcurrency, 
                                  MContext.AppConfig.NumberOfApiMessageRetries));

                await MContext.DataStore.CommitChanges();

                MContext.Logger.Debug($"Created record with id {newItem.id} for msg id {message.MessageId}");

                outLogEntry(newItem.Clone());
            }
            catch (Exception e)
            {
                throw new Exception($"Could not write message {message.MessageId} to store", e);
            }
        }

        internal static async Task CreateOrFindLogEntry(this ApiMessage message, Action<MessageLogEntry> outLogEntry)
        {
            MessageLogEntry entry = null;
            await message.FindLogEntry(v => entry = v);
            if (entry == null) await message.CreateNewLogEntry(v => entry = v);
        }

        internal static async Task FindLogEntry(this ApiMessage message, Action<MessageLogEntry> outResult)
        {
            try
            {
                MContext.Logger.Debug($"Looking for msg id {message.MessageId}");

                var result = await MContext.DataStore.ReadActiveById<MessageLogEntry>(message.MessageId);

                MContext.Logger.Debug(
                    result == null
                        ? $"Failed to find record for msg id {message.MessageId}"
                        : $"Found record with id {result.id} for msg with id {message.MessageId}");

                outResult(result);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not read message {message.MessageId} from store", e);
            }
        }

        internal static async Task MarkFailureInMessageLog(this ApiMessage message, FormattedExceptionInfo exceptionInfo)
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
                MContext.MessageAggregator.Clear();

                if (ThisFailureIsTheFinalFailure() && !TheMessageWeAreProcessingIsAMaxFailNotificationMessage())
                {
                    /* send a message to the bus which tells us this has occured
                     allowing us to handle these cases with compensating logic */
                    SendFinalFailureMessage();
                }

                await AddThisFailureToTheMessageLog();

                await QueuedStateChanges.CommitChanges();
            }

            catch (Exception e)
            {
                throw new Exception($"Error logging failed message {message.MessageId} result to DataStore", e);
            }

            async Task AddThisFailureToTheMessageLog()
            {
                //- in-place update, unlikely to be used, better safe than sorry
                var logEntry = MContext.AfterMessageLogEntryObtained.MessageLogEntry;
                logEntry.AddFailedAttempt(exceptionInfo);

                await MContext.DataStore.Update(logEntry);
            }

            bool TheMessageWeAreProcessingIsAMaxFailNotificationMessage()
            {
                //- avoid infinite loop
                return message.GetType().InheritsOrImplements(typeof(MessageFailedAllRetries));
            }

            bool ThisFailureIsTheFinalFailure()
            {
                //- remember that total attempts is initial message + retries
                return MContext.AfterMessageLogEntryObtained.MessageLogEntry.Attempts.Count
                       == MContext.AppConfig.NumberOfApiMessageRetries;
            }

            void SendFinalFailureMessage()
            {
                var genericTypeWithParam = typeof(MessageFailedAllRetries<>).MakeGenericType(message.GetType());

                var instanceOfMesageFailedAllRetries =
                    (MessageFailedAllRetries)Activator.CreateInstance(genericTypeWithParam, message.MessageId);

                instanceOfMesageFailedAllRetries.MessageId = Guid.NewGuid();
                instanceOfMesageFailedAllRetries.TimeOfCreationAtOrigin = DateTime.UtcNow;
                instanceOfMesageFailedAllRetries.IdOfMessageThatFailed = message.MessageId;

                if (message is ApiCommand command)
                {
                    instanceOfMesageFailedAllRetries.StatefulProcessIdOfMessageThatFailed = command.StatefulProcessId;
                }

                MContext.Bus.Send(instanceOfMesageFailedAllRetries);
            }
        }

        internal static void SerilogFailure(this ApiMessage message, FormattedExceptionInfo exceptionInfo)
        {
            var meta = MContext.AfterMessageLogEntryObtained.MessageMeta;

            var serilogEntry = new FailedAttempt(exceptionInfo)
            {
                SapiReceivedAt = meta?.ReceivedAt,
                SapiCompletedAt = DateTime.UtcNow,
                UserName = meta?.RequestedBy?.UserName,
                MessageId = message.MessageId,
                Schema = message.GetType().FullName,
                Message = message,
                IsCommand = message is ApiCommand,
                IsQuery = message is IApiQuery,
                IsEvent = message is ApiEvent,
                ProfilingData = meta != null ? CreateProfilingData(meta) : null,
                EnvironmentName = MContext.AppConfig.EnvironmentName,
                ApplicationName = MContext.AppConfig.ApplicationName
            };

            MContext.Logger.Error("Message: {@message}", serilogEntry);
        }

        internal static void SerilogSuccess(this ApiMessage message)
        {
            var meta = MContext.AfterMessageLogEntryObtained.MessageMeta;

            var serilogEntry = new SuccessfulAttempt
            {
                Succeeded = true,
                SapiReceivedAt = meta.ReceivedAt,
                SapiCompletedAt = DateTime.UtcNow,
                UserName = meta.RequestedBy?.UserName,
                MessageId = message.MessageId,
                Schema = meta.Schema,
                Message = message,
                IsCommand = message is ApiCommand,
                IsQuery = message is IApiQuery,
                IsEvent = message is ApiEvent,
                ProfilingData = CreateProfilingData(meta),
                EnvironmentName = MContext.AppConfig.EnvironmentName,
                ApplicationName = MContext.AppConfig.ApplicationName
            };

            MContext.Logger.Information("Message: {@message}", serilogEntry);
        }

        internal static  void  ValidateOrThrow(this ApiMessage message)
        {
            {
                var messageLogEntry = MContext.AfterMessageLogEntryObtained.MessageLogEntry;

                Guard.Against(message.MessageId == Guid.Empty, "All ApiMessages must have a unique MessageId property value");

                Guard.Against(
                    IsADifferentMessageButWithTheSameId(messageLogEntry),
                    GlobalErrorCodes.ItemIsADifferentMessageWithTheSameId);

                Guard.Against(
                    HasAlreadyBeenProcessedSuccessfully(messageLogEntry),
                    GlobalErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully);

                Guard.Against(
                    HasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogEntry),
                    GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes);

                message.Validate();
            }

            bool HasAlreadyBeenProcessedSuccessfully(MessageLogEntry messageLogEntry)
            {
                //- safeguard, cannot think of a reason it would happen 
                return messageLogEntry.UnitOfWork != null && messageLogEntry.ProcessingComplete;
            }

            bool HasAlreadyFailedTheMaximumNumberOfTimesAllowed(MessageLogEntry messageLogEntry)
            {
                //should never happen, unless message broker/bus it configured to retry message more times
                //than the pipeline is configured to allow
                return messageLogEntry.Attempts.Count > MContext.AppConfig.NumberOfApiMessageRetries;
            }

            bool IsADifferentMessageButWithTheSameId(MessageLogEntry messageLogEntry)
            {
                var messageAsJson = JsonSerializer.Serialize(message);
                var hashMatches = messageAsJson.Verify(messageLogEntry.MessageHash);
                return !hashMatches;
            }
        }

        private static object CreateProfilingData(MessageMeta meta)
        {
            {
                var initialTimestamp = meta.StartTicks;

                var finalTimestamp = StopwatchOps.GetStopwatchTimestamp();

                var previousTimestamp = initialTimestamp;

                var stateOperations = new List<object>();

                foreach (var stateOperation in MContext.MessageAggregator.AllMessages.OfType<IStateOperation>())
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
    }
}