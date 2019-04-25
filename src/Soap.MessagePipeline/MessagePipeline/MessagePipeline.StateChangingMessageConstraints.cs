namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Transactions;
    using DataStore.Interfaces;
    using Newtonsoft.Json;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.Utility;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public partial class MessagePipeline
    {
        private static class StateChangingMessageConstraints
        {
            private static readonly object ValidationLocker = new object();

            /// <summary>
            ///     keep in mind you cannot await in the body of a lock statement
            /// </summary>
            internal static void Enforce(IDataStore dataStore, IApiMessage message, IApplicationConfig appConfig, ILogger logger, out MessageLogItem logItem)
            {
                {
                    try
                    {
                        //prevent the same message on other threads passing duplicate validation checks due to a race condition
                        lock (ValidationLocker)
                        {
                            logger.Debug($"Lock acquired, starting unique enforcement for msg {message.MessageId}");
                            logItem = Validate() ?? CreateNewLogEntry();
                        }
                    }
                    catch (Exception exception)
                    {
                        /* messagelog ignored here, as db problems are most likely cause of failure, 
                         * and duplicates would append to the previous message instance
                         * finally, it will be as if the message was never received from the messagelog point of view
                        */
                        var pipelineMessages = PipelineExceptionMessages.Create(exception, appConfig, message, null);

                        var serilogEntry = new FailedMessageLogEntry
                        {
                            SapiReceivedAt = DateTime.UtcNow,
                            SapiCompletedAt = null,
                            UserName = null,
                            MessageId = message.MessageId,
                            Schema = message.GetType().FullName,
                            Message = message,
                            IsCommand = message is IApiCommand,
                            IsQuery = message is IApiQuery,
                            IsEvent = message is IApiEvent,
                            ProfilingData = null,
                            ExceptionMessages = pipelineMessages,
                            EnvironmentName = appConfig.EnvironmentName,
                            ApplicationName = appConfig.ApplicationName
                        };

                        logger.Error("Message: {@message}", serilogEntry);

                        throw pipelineMessages.ToEnvironmentSpecificError(appConfig);
                    }
                }

                MessageLogItem Validate()
                {
                    {
                        Guard.Against(message.MessageId == Guid.Empty, "All ApiMessages must have a unique MessageId property value");

                        FindMessageLogItem(out MessageLogItem messageLogItem);

                        if (ItemHasAlreadyBeenReceived(messageLogItem))
                        {
                            Guard.Against(
                                ItemIsADifferentMessageWithTheSameId(messageLogItem),
                                GlobalErrorCodes.ItemIsADifferentMessageWithTheSameId);

                            Guard.Against(MessageHasAlreadyBeenProcessedSuccessfully(messageLogItem),
                              GlobalErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully);

                            Guard.Against(
                                MessageHasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogItem),
                                GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes);

                            return messageLogItem;
                        }
                        return null;
                    }

                    bool MessageHasAlreadyBeenProcessedSuccessfully(MessageLogItem messageLogItem)
                    {
                        //safeguard, should never happen, would be a bug if it did
                        return messageLogItem.SuccessfulAttempt != null;
                    }

                    bool MessageHasAlreadyFailedTheMaximumNumberOfTimesAllowed(MessageLogItem messageLogItem)
                    {
                        //should never happen, unless message broker/bus it configured to retry message more times
                        //than the pipeline is configured to allow
                        return messageLogItem.FailedAttempts.Count > appConfig.NumberOfApiMessageRetries;
                    }

                    bool ItemIsADifferentMessageWithTheSameId(MessageLogItem messageLogItem)
                    {
                        var messageAsJson = JsonConvert.SerializeObject(message);
                        var hashMatches = Md5Hash.Verify(messageAsJson, messageLogItem.MessageHash);
                        return !hashMatches;
                    }

                    bool ItemHasAlreadyBeenReceived(MessageLogItem messageLogItemMatch)
                    {
                        return messageLogItemMatch != null;
                    }

                    void FindMessageLogItem(out MessageLogItem result)
                    {
                        try
                        {
                            logger.Debug($"Looking for msg id {message.MessageId}");

                            result = dataStore.ReadActiveById<MessageLogItem>(message.MessageId).GetAwaiter().GetResult();

                            logger.Debug(
                                result == null
                                    ? $"Failed to find record for msg id {message.MessageId}"
                                    : $"Found record with id {result.id} for msg with id {message.MessageId}");
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Could not read message {message.MessageId} from store", e);
                        }
                    }
                }

                MessageLogItem CreateNewLogEntry()
                {
                    try
                    {
                        //save this immediately outside of the ambient txn so other threads will not get passed duplicate check
                        using (var tx = new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            logger.Debug($"Creating record for msg id {message.MessageId}");

                            var newItem = dataStore.Create(MessageLogItem.Create(message, appConfig)).GetAwaiter().GetResult();

                            dataStore.CommitChanges().GetAwaiter().GetResult();

                            tx.Complete();

                            logger.Debug($"Created record with id {newItem.id} for msg id {message.MessageId}");

                            return newItem.Clone();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Could not write message {message.MessageId} to store", e);
                    }
                }
            }
        }
    }
}