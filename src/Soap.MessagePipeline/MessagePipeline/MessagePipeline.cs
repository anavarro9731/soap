namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessageAggregator;
    using Soap.If.MessagePipeline.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    /// <summary>
    ///     The jobs of this class is are:
    ///     1. Add MetaData to the message
    ///     2. Map it to a specific handler type
    ///     3. Log the overall result of the attempted operation.
    ///     |
    ///     There are 5 types of exceptions that could occur.
    ///     Domain Exceptions (failed guards), Validation Exceptions(message syntax failures), Exceptions handling Exceptions,
    ///     UniqueConstraint violations, and Unexpected exceptions (any other exceptions thrown at any level inside the
    ///     pipeline)
    ///     |
    ///     And these are handled at the top level of the pipeline with 3 potential clients in mind
    ///     1. External clients, including unit tests, which will receive only a single string error message with more or less
    ///     detail depending on Environment
    ///     for all 4 types of exceptions
    ///     2. Users of the Serilog logging system, which will receive the ExceptionMessages class serialised as part of the
    ///     failed message log entry
    ///     for all but the Exceptions handling Exceptions and UniqueConstraint violations
    ///     for which they will receive a single line Fatal error
    ///     3. Domain code which queries the message log to execute domain logic based a messages failed result.
    ///     They will see a serialised verison of the ExceptionMessages class.
    ///     No information is logged to the message log about exceptions which occur while handling an exception.
    ///     In the event of an exception while handling an exception whether the failed message result
    ///     will be logged to the message log or not is entirely dependent on what the second exception was.
    /// </summary>
    public partial class MessagePipeline 
    {
        private readonly IApplicationConfig appConfig;

        private readonly IAuthenticateUsers authenticator;

        private readonly IBusContext busContext;

        private readonly IDataStore dataStore;

        private readonly IList<IMessageHandler> handlers;

        private readonly ILogger logger;

        private readonly IMessageAggregator messageAggregator;

        public MessagePipeline(
            IApplicationConfig appConfig,
            ILogger logger,
            IMessageAggregator messageAggregator,
            IList<IMessageHandler> handlers,
            IAuthenticateUsers authenticator,
            IDataStore dataStore,
            IBusContext busContext)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            this.messageAggregator = messageAggregator;
            this.handlers = handlers;
            this.authenticator = authenticator;
            this.dataStore = dataStore;
            this.busContext = busContext;
        }

        public async Task<object> Execute(IApiMessage message)
        {
            var receivedAtTimestamp = StopwatchUtil.GetStopwatchTimestamp();
            var receivedAt = DateTime.UtcNow;

            {
                //if this block fails, errors should be logged to serilog,
                //but message will not be logged to messageresults
                MessageLogItem messageLogItem = null;

                var meta = new ApiMessageMeta
                {
                    ReceivedAtTimestamp = receivedAtTimestamp,
                    ReceivedAt = receivedAt
                };

                if (message.CanChangeState())
                {
                    //todo: create as pipelineexception message
                    StateChangingMessageConstraints.Enforce(this.dataStore, message, this.appConfig, this.logger, out messageLogItem);
                }

                try //execute the message
                {
                    CreateMeta(messageLogItem, out meta);

                    FindHandlers(out List<IMessageHandler> matchingHandlers);

                    if (MessageIsFailedAllRetriesMessageWithoutAHandler(matchingHandlers))
                    {
                        LogSuccessfulMessage(message, meta, null);
                        return null;
                    }

                    FindHandlerOrThrow(matchingHandlers, out IMessageHandler handler);

                    Guard.Against(
                        message.MessageId == Constants.ForceFailBeforeMessageCompletesId
                        || message.MessageId == Constants.ForceFailBeforeMessageCompletesAndFailErrorHandlerId,
                        "Forced Fail In Message Handler",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                    var result = await handler.HandleAny(message, meta).ConfigureAwait(false);

                    LogSuccessfulMessage(message, meta, result);

                    return result;
                }
                catch (Exception exception)
                {
                    Exception finalException;

                    try //log the message failure
                    {
                        var exceptionMessages = PipelineExceptionMessages.Create(exception, this.appConfig, message);

                        await LogFailedMessage(message, exceptionMessages, messageLogItem, meta).ConfigureAwait(false);

                        Guard.Against(
                            message.MessageId == Constants.ForceFailBeforeMessageCompletesAndFailErrorHandlerId,
                            "Forced Fail In Exception Handler",
                            ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                        finalException = exceptionMessages.ToEnvironmentSpecificError(this.appConfig);
                    }
                    catch (Exception exceptionHandlingException)
                    {
                        try //log the second level error with some detail
                        {
                            var orignalExceptionPlusHandlingException =
                                new ExceptionHandlingException(new AggregateException(exceptionHandlingException, exception));

                            var exceptionMessages = PipelineExceptionMessages.Create(orignalExceptionPlusHandlingException, this.appConfig, message);

                            this.logger.Error("Error Handling Failed Message {@error}", exceptionMessages);

                            finalException = exceptionMessages.ToEnvironmentSpecificError(this.appConfig);
                        }
                        catch //log a minimal error message of last resort
                        {
                            //avoid use of pipelinemessages here, this goes raw to the client don't show any exception details
                            //serilog should never throw an error itself on the main thread, so logging should be safe here
                            var lastChanceExceptionMessage = $"{PipelineExceptionMessages.CodePrefixes.EXWHEX}: {this.appConfig.DefaultExceptionMessage}";

                            this.logger.Error(lastChanceExceptionMessage);

                            finalException = new Exception(lastChanceExceptionMessage);
                        }
                    }

                    throw finalException;
                }
            }

            void CreateMeta(MessageLogItem messageLogItem, out ApiMessageMeta meta)
            {
                meta = new ApiMessageMeta
                {
                    ReceivedAtTimestamp = receivedAtTimestamp,
                    ReceivedAt = receivedAt,
                    Schema = message.GetType().FullName,
                    RequestedBy = message.IdentityToken != null ? this.authenticator.Authenticate(message) : null,
                    MessageLogItem = messageLogItem
                };
            }

            void FindHandlerOrThrow(List<IMessageHandler> matchingHandlers, out IMessageHandler handler)
            {
                Guard.Against(matchingHandlers.Count() > 1, $"Could not map message {message.MessageId} to handler, as more than one exists for this message type.");

                Guard.Against(!matchingHandlers.Any(), $"Could not map message {message.MessageId} to handler, as none exists for this message type. {message.GetType().FullName}");

                handler = matchingHandlers.Single();
            }

            void FindHandlers(out List<IMessageHandler> matchingHandlers)
            {
                var messageType = message.GetType().FullName;
                var messageReturnType = message.GetType().BaseType.GenericTypeArguments.FirstOrDefault()?.FullName;

                // THandler<TMessage,TReturn> == TMessage<TReturn>
                matchingHandlers = this.handlers.Where(
                                               h =>
                                                   {
                                                       Type handlerType = h.GetType();

                                                       bool TypeHasOnlyTheMessageAndOrReturnTypeParams(Type t) => t.IsGenericType &&
                                                                                                                  t.GenericTypeArguments.Length <= 2;

                                                       while (!TypeHasOnlyTheMessageAndOrReturnTypeParams(handlerType))
                                                       {
                                                           handlerType = handlerType.BaseType;
                                                       }

                                                       var handlerMessageType = handlerType.GenericTypeArguments.First().FullName;
                                                       var handlerReturnType = handlerType.GenericTypeArguments.Length == 2 ?
                                                                                   handlerType.GenericTypeArguments[1].FullName : null;

                                                       return messageType == handlerMessageType && messageReturnType == handlerReturnType;
                                                   }).ToList();
            }

            bool MessageIsFailedAllRetriesMessageWithoutAHandler(List<IMessageHandler> matchingHandlers)
            {
                return (message is IMessageFailedAllRetries && matchingHandlers.Count == 0);

            }
        }

        private object CreateProfilingData(ApiMessageMeta meta)
        {
            {
                var initialTimestamp = meta.ReceivedAtTimestamp;

                var finalTimestamp = StopwatchUtil.GetStopwatchTimestamp();

                var previousTimestamp = initialTimestamp;

                var stateOperations = new List<object>();

                foreach (var stateOperation in this.messageAggregator.AllMessages.OfType<IStateOperation>())
                {
                    if (HasValidTimestamps(stateOperation, previousTimestamp) == false)
                    {
                        continue;
                    }

                    stateOperations.Add(
                        new
                        {
                            Duration = StopwatchUtil.CalculateLatency(previousTimestamp, stateOperation.StateOperationStartTimestamp).ToString(),
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
                                            + dataStoreOperation.TypeName.SubstringAfterLast('.') + (dataStoreOperation.TypeName != null ? ">" : "")
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
                        Duration = StopwatchUtil.CalculateLatency(previousTimestamp, finalTimestamp).ToString(),
                        Name = "CPU Operation(s)"
                    });

                var profilingData = new
                {
                    TotalProcessingTime = StopwatchUtil.CalculateLatency(initialTimestamp, finalTimestamp),
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

        private async Task LogFailedMessage(
            IApiMessage message,
            PipelineExceptionMessages pipelineExceptionMessages,
            MessageLogItem messageLogItem,
            ApiMessageMeta meta)
        {
            {
                var serilogEntry = new FailedMessageLogEntry
                {
                    SapiReceivedAt = meta.ReceivedAt,
                    SapiCompletedAt = DateTime.UtcNow,
                    UserName = meta.RequestedBy?.UserName,
                    MessageId = message.MessageId,
                    Schema = message.GetType().FullName,
                    Message = message,
                    IsCommand = message is IApiCommand,
                    IsQuery = message is IApiQuery,
                    IsEvent = message is IApiEvent,
                    ProfilingData = CreateProfilingData(meta),
                    ExceptionMessages = pipelineExceptionMessages,
                    EnvironmentName = this.appConfig.EnvironmentName,
                    ApplicationName = this.appConfig.ApplicationName
                };

                this.logger.Error("Message: {@message}", serilogEntry);

                if (message.CanChangeState())
                {
                    await LoggingForStateChangingMessages().ConfigureAwait(false);
                }
            }

            async Task LoggingForStateChangingMessages()
            {
                try
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        /* Commit immediately and outside of the ambient txn otherwise any changes would be rolled back when
                         * the ambient transaction aborts
                         * 
                         * It is possible we could fail while handling an error'd message but before we hit this block
                         * in that instance we will fail to record the error and the message's error count will remain the same.
                         * Any changes made by the message will of course be rolled back by the ambient txn, but the message will
                         * be retried again as if it was the first failure. This could at times result in a number of messages
                         * being retried indefinately until the underlying problem is fixed.
                         * 
                         * It is also possible we could fail after writing this block. Again any changes made by the message 
                         * will of course be rolled back by the ambient txn, but the result will likely be a different error
                         * would be raised to the calling service, or no error in the case of a machine losing power.
                        */

                        if (ThisFailureIsTheFinalFailure() && 
                            !TheMessageWeAreProcessingIsAMaxFailNotificationMessage() &&
                            !this.busContext.IsOneWay)
                        {
                            //include a message to the bus which tells us this has occured and 
                            //allows us to handle these cases with additional logic
                            await SendFinalFailureMessage().ConfigureAwait(false);
                        }

                        RemoveQueuedOperations();
                        await AddThisFailureToTheMessageLog().ConfigureAwait(false);
                        await this.dataStore.CommitChanges().ConfigureAwait(false);
                        scope.Complete();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error logging failed message {message.MessageId} result to DataStore", e);
                }
            }

            void RemoveQueuedOperations()
            {
                //BUG: circuitboard should really be changed to support queuops
                //right now this only works on the real messageaggregator not IMessageaggregator
                //so testing versions of message aggregator will end up committing queuedopertions on a failure
                //essentially NOT rolling back the code
                bool MessageIsQueuedStateChange(IMessage m) => m is IQueuedStateChange change && change.Committed == false;
                (this.messageAggregator as MessageAggregator)?.RemoveWhere(MessageIsQueuedStateChange);
            }

            async Task AddThisFailureToTheMessageLog()
            {

                await this.dataStore.UpdateById<MessageLogItem>(
                              messageLogItem.Id,
                              obj => MessageLogItemOperations.AddFailedMessageResult(obj, pipelineExceptionMessages))
                          .ConfigureAwait(false);
            }

            bool TheMessageWeAreProcessingIsAMaxFailNotificationMessage()
            {
                //avoid infinite loop
                return message.GetType().InheritsOrImplements(typeof(IMessageFailedAllRetries));
            }

            bool ThisFailureIsTheFinalFailure()
            {
                return messageLogItem.FailedAttempts.Count == this.appConfig.NumberOfApiMessageRetries;
            }

            async Task SendFinalFailureMessage()
            {
                var genericTypeWithParam = typeof(MessageFailedAllRetries<>).MakeGenericType(message.GetType());

                var instanceOfMesageFailedAllRetries = (IMessageFailedAllRetries)Activator.CreateInstance(genericTypeWithParam, message.MessageId);

                instanceOfMesageFailedAllRetries.MessageId = Guid.NewGuid();
                instanceOfMesageFailedAllRetries.TimeOfCreationAtOrigin = DateTime.UtcNow;
                instanceOfMesageFailedAllRetries.IdOfMessageThatFailed = message.MessageId;

                if (message is IApiCommand command)
                {
                    instanceOfMesageFailedAllRetries.StatefulProcessIdOfMessageThatFailed = command.StatefulProcessId;
                }

                await this.busContext.SendLocal(instanceOfMesageFailedAllRetries).ConfigureAwait(false);
            }
        }

        private void LogSuccessfulMessage(IApiMessage message, ApiMessageMeta meta, object result)
        {
            {
                var serilogEntry = new SuccessfulMessageLogEntry
                {
                    SapiReceivedAt = meta.ReceivedAt,
                    SapiCompletedAt = DateTime.UtcNow,
                    UserName = meta.RequestedBy?.UserName,
                    MessageId = message.MessageId,
                    Schema = meta.Schema,
                    Message = message,
                    Result = result,
                    IsCommand = message is IApiCommand,
                    IsQuery = message is IApiQuery,
                    IsEvent = message is IApiEvent,
                    ProfilingData = CreateProfilingData(meta),
                    EnvironmentName = this.appConfig.EnvironmentName,
                    ApplicationName = this.appConfig.ApplicationName
                };

                this.logger.Information("Message: {@message}", serilogEntry);
            }
        }
    }
}