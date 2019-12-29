namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Transactions;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessageAggregator;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.MessageContractsBase.Commands;

    public static class IApiMessageExts
    {
        internal static async Task EnforceConstraints(this ApiMessage message, Action<MessageLogEntry> outLogItem)
        {
            { 
                try
                {
                    outLogItem(await Validate() ?? await CreateNewLogEntry());
                }
                catch (Exception exception)
                {
                    var pipelineMessages = LogMessageFailureEarlyAndBail(exception);

                    throw pipelineMessages.ToEnvironmentSpecificError();

                }
            }

            MessageExceptionInfo LogMessageFailureEarlyAndBail(Exception exception)
            {
                /* Formal MessageLog ignored here, as db problems are most likely cause of failure, 
                 * and duplicates would append to the previous message instance
                 * finally, it will be as if the message was never received from the MessageLog's point of view
                */
                var pipelineMessages = new MessageExceptionInfo(exception, message);

                var serilogEntry = new FailedMessageLogItem(pipelineMessages)
                {
                    SapiReceivedAt = DateTime.UtcNow,
                    SapiCompletedAt = null,
                    UserName = null,
                    MessageId = message.MessageId,
                    Schema = message.GetType().FullName,
                    Message = message,
                    IsCommand = message is ApiCommand,
                    IsQuery = message is IApiQuery,
                    IsEvent = message is ApiEvent,
                    ProfilingData = null,
                    EnvironmentName = MMessageContext.AppConfig.EnvironmentName,
                    ApplicationName = MMessageContext.AppConfig.ApplicationName
                };

                MMessageContext.Logger.Error("Message: {@message}", serilogEntry);
                return pipelineMessages;
            }

            async Task<MessageLogEntry> Validate()
            {
                {
                    Guard.Against(message.MessageId == Guid.Empty, "All ApiMessages must have a unique MessageId property value");

                    MessageLogEntry messageLogItem = null;

                    await FindMessageLogItem(v => messageLogItem = v);

                    if (HasAlreadyBeenReceived(messageLogItem))
                    {
                        Guard.Against(IsADifferentMessageButWithTheSameId(messageLogItem), GlobalErrorCodes.ItemIsADifferentMessageWithTheSameId);

                        Guard.Against(HasAlreadyBeenProcessedSuccessfully(messageLogItem), GlobalErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully);

                        Guard.Against(HasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogItem), GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes);

                        return messageLogItem;
                    }

                    return null;
                }

                bool HasAlreadyBeenProcessedSuccessfully(MessageLogEntry messageLogItem)
                {
                    //safeguard, should never happen, would be a bug if it did
                    return messageLogItem.SuccessfulAttempt != null;
                }

                bool HasAlreadyFailedTheMaximumNumberOfTimesAllowed(MessageLogEntry messageLogItem)
                {
                    //should never happen, unless message broker/bus it configured to retry message more times
                    //than the pipeline is configured to allow
                    return messageLogItem.FailedAttempts.Count > MMessageContext.AppConfig.NumberOfApiMessageRetries;
                }

                bool IsADifferentMessageButWithTheSameId(MessageLogEntry messageLogItem)
                {
                    var messageAsJson = JsonSerializer.Serialize(message);
                    var hashMatches = Md5HashExt.Verify(messageAsJson, messageLogItem.MessageHash);
                    return !hashMatches;
                }

                bool HasAlreadyBeenReceived(MessageLogEntry messageLogItemMatch)
                {
                    return messageLogItemMatch != null;
                }

                async Task FindMessageLogItem(Action<MessageLogEntry> outResult)
                {
                    try
                    {
                        MMessageContext.Logger.Debug($"Looking for msg id {message.MessageId}");

                        var result = await MMessageContext.DataStore.ReadActiveById<MessageLogEntry>(message.MessageId);

                        MMessageContext.Logger.Debug(
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
            }

            async Task<MessageLogEntry> CreateNewLogEntry()
            {
                try
                {
                    MMessageContext.Logger.Debug($"Creating record for msg id {message.MessageId}");

                    var newItem = await MMessageContext.DataStore.Create(new MessageLogEntry(message));

                    await MMessageContext.DataStore.CommitChanges();

                    MMessageContext.Logger.Debug($"Created record with id {newItem.id} for msg id {message.MessageId}");

                    return newItem.Clone();
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not write message {message.MessageId} to store", e);
                }
            }
        }

        internal static async Task MarkSuccess(this ApiMessage message)
        {
            var meta = MMessageContext.AfterMessageAccepted.MessageMeta;

            var serilogEntry = new SuccessfulLogEntry()
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
                EnvironmentName = MMessageContext.AppConfig.EnvironmentName,
                ApplicationName = MMessageContext.AppConfig.ApplicationName
            };

            await MMessageContext.DataStore.UpdateById<MessageLogEntry>(message.MessageId, logEntry =>
                logEntry.AddSuccessfulMessageResult());

            MMessageContext.Logger.Information("Message: {@message}", serilogEntry);
        }

        private static object CreateProfilingData(MessageMeta meta)
        {
            {
                var initialTimestamp = meta.StartTicks;

                var finalTimestamp = StopwatchOps.GetStopwatchTimestamp();

                var previousTimestamp = initialTimestamp;

                var stateOperations = new List<object>();

                foreach (var stateOperation in MMessageContext.MessageAggregator.AllMessages.OfType<IStateOperation>())
                {
                    if (HasValidTimestamps(stateOperation, previousTimestamp) == false)
                    {
                        continue;
                    }

                    stateOperations.Add(
                        new
                        {
                            Duration = StopwatchOps.CalculateLatency(previousTimestamp, stateOperation.StateOperationStartTimestamp).ToString(),
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

                    if (IsStopTimestampBeforeStartTimestamp(stateOperation.StateOperationStartTimestamp, stateOperation.StateOperationStopTimestamp))
                    {
                        return false;
                    }

                    if (IsStartTimestampBeforePrevoiusStopTimestamp(previousStateOperationStopTimestamp, stateOperation.StateOperationStartTimestamp))
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

        internal static async Task MarkFailure(
            this ApiMessage message,
            MessageExceptionInfo pipelineExceptionMessages,
            MessageLogEntry messageLogItem)
        {
            {
                var meta = MMessageContext.AfterMessageAccepted.MessageMeta;
                var serilogEntry = new FailedMessageLogItem(pipelineExceptionMessages)
                {
                    SapiReceivedAt = meta.ReceivedAt,
                    SapiCompletedAt = DateTime.UtcNow,
                    UserName = meta.RequestedBy?.UserName,
                    MessageId = message.MessageId,
                    Schema = message.GetType().FullName,
                    Message = message,
                    IsCommand = message is ApiCommand,
                    IsQuery = message is IApiQuery,
                    IsEvent = message is ApiEvent,
                    ProfilingData = CreateProfilingData(meta),
                    EnvironmentName = MMessageContext.AppConfig.EnvironmentName,
                    ApplicationName = MMessageContext.AppConfig.ApplicationName
                };

                MMessageContext.Logger.Error("Message: {@message}", serilogEntry);

                if (message.CanChangeState)
                {
                    await LogStateChangingMessage();
                }
            }

            async Task LogStateChangingMessage()
            {
                try
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

                    if (ThisFailureIsTheFinalFailure() && !TheMessageWeAreProcessingIsAMaxFailNotificationMessage())
                    {
                        /* send a message to the bus which tells us this has occured
                         allowing us to handle these cases with compensating logic */
                        SendFinalFailureMessage();
                    }

                    await AddThisFailureToTheMessageLog();

                    await MMessageContext.DataStore.CommitChanges();
                }
                catch (Exception e)
                {
                    throw new Exception($"Error logging failed message {message.MessageId} result to DataStore", e);
                }
            }

            async Task AddThisFailureToTheMessageLog()
            {
                await MMessageContext.DataStore.UpdateById<MessageLogEntry>(messageLogItem.id, logEntry =>
                    logEntry.AddFailedMessageResult(pipelineExceptionMessages));
            }

            bool TheMessageWeAreProcessingIsAMaxFailNotificationMessage()
            {
                //- avoid infinite loop
                return message.GetType().InheritsOrImplements(typeof(MessageFailedAllRetries));
            }

            bool ThisFailureIsTheFinalFailure()
            {
                return messageLogItem.FailedAttempts.Count == MMessageContext.AppConfig.NumberOfApiMessageRetries;
            }

            void SendFinalFailureMessage()
            {
                var genericTypeWithParam = typeof(MessageFailedAllRetries<>).MakeGenericType(message.GetType());

                var instanceOfMesageFailedAllRetries = (MessageFailedAllRetries)Activator.CreateInstance(genericTypeWithParam, message.MessageId);

                instanceOfMesageFailedAllRetries.MessageId = Guid.NewGuid();
                instanceOfMesageFailedAllRetries.TimeOfCreationAtOrigin = DateTime.UtcNow;
                instanceOfMesageFailedAllRetries.IdOfMessageThatFailed = message.MessageId;

                if (message is ApiCommand command)
                {
                    instanceOfMesageFailedAllRetries.StatefulProcessIdOfMessageThatFailed = command.StatefulProcessId;
                }

                MMessageContext.Bus.SendCommand(instanceOfMesageFailedAllRetries);
            }
        }
    }
}