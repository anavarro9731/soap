namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Transactions;
    using DataStore.Interfaces;
    using DataStore.Models.PureFunctions.Extensions;
    using Newtonsoft.Json;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.Models.Aggregates;
    using Soap.Utility;
    using Soap.Utility.PureFunctions;

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
                            logger.Debug($"Lock acquired, starting unqiue enforcement for msg {message.MessageId}");
                            logItem = Validate() ?? CreateNewLogEntry();
                        }
                    }
                    catch (Exception exception)
                    {
                        //messagelog ignored here, as db problems are most likely cause of failure, 
                        //it will be as if the message was never received from messagelog point of view

                        var exceptionMessage = $"{PipelineExceptionMessages.CodePrefixes.INVALID}: {exception.Message}";

                        logger.Error(exception, exceptionMessage);

                        //top level message raised below should all be safe for the client as this message goes raw to the client
                        throw new Exception(exceptionMessage);
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
                                $"A message with this MessageId {message.MessageId} has already been processed. This message will be discarded. If this is not a duplicate message please resend with a unique MessageId property value");

                            Guard.Against(MessageHasAlreadyBeenProcessedSuccessfully(messageLogItem), "This message has already been processed successfully");

                            Guard.Against(
                                MessageHasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogItem),
                                $"Message {message.MessageId} had already failed the maximum number of times allowed");

                            return messageLogItem;
                        }
                        return null;
                    }

                    bool MessageHasAlreadyBeenProcessedSuccessfully(MessageLogItem messageLogItem)
                    {
                        //safeguard, should never happen, would be abug if it did
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
                        return Md5Hash.Verify(messageAsJson, messageLogItem.MessageHash) == false;
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
                        using (var tx = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
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