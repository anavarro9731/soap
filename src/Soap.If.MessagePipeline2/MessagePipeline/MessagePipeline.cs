namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    /// <summary>
    ///     1. Build MetaData to the message
    ///     2. Map it to a specific handler type
    ///     3. Log the overall result of the attempted operation.
    /// </summary>
    public class MessagePipeline
    {
        private readonly IAuthenticateUsers authenticator;

        private readonly object handlerClass;

        public MessagePipeline(IAuthenticateUsers authenticator, object handlerClass)
        {
            this.authenticator = authenticator;
            this.handlerClass = handlerClass;
        }

        public async Task Execute(string messageJson, string assemblyQualifiedName)
        {
            { 
                var receivedAtTick = StopwatchOps.GetStopwatchTimestamp();
                var receivedAt = DateTime.UtcNow;
                MessageLogEntry messageLogItem = null;

                ApiMessage message = JsonSerializer.Deserialize(messageJson, Type.GetType(assemblyQualifiedName)).As<ApiMessage>();

                if (message.CanChangeState)
                {
                    /* if this constraints fail
                     errors are logged to Serilog 
                     but the message is not logged to MessageResults */
                    await message.EnforceConstraints(v => messageLogItem = v);

                    //TODO: check to see if message has unit of work waiting and do that instead
                }

                try //- execute the message
                {
                    message.Validate();

                    SetMessageMetaInContext(messageLogItem, message, (receivedAt, receivedAtTick));

                    if (message.IsFailedAllRetriesMessage)
                    {
                        message.HandleFinalFailure();
                    }
                    else
                    {
                        switch (message)
                        {
                            case IApiQuery q:
                                var responseEvent = await q.Handle();
                                //TODO queue response event
                                break;
                            case ApiCommand c:
                                await c.Handle();
                                break;
                            case ApiEvent e:
                                await e.Handle();
                                break;
                        }
                    }

                    await QueuedStateChanges.CommitChanges();

                    Guard.Against(
                        message.MessageId == Constants.ForceFailBeforeMessageCompletesId
                        || message.MessageId == Constants.ForceFailBeforeMessageCompletesAndFailErrorHandlerId,
                        "Forced Fail In Message Handler",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                    //TODO: move successlogentry into write queue?
                    await message.MarkSuccess();
                }
                catch (Exception exception)
                {
                    Exception finalException;

                    try //- log the message failure
                    {
                        var exceptionMessages = new MessageExceptionInfo(exception, message);

                        await message.MarkFailure(exceptionMessages, messageLogItem);

                        Guard.Against(
                            message.MessageId == Constants.ForceFailBeforeMessageCompletesAndFailErrorHandlerId,
                            "Forced Fail In Exception Handler",
                            ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                        finalException = exceptionMessages.ToEnvironmentSpecificError();
                    }
                    catch (Exception exceptionHandlingException)
                    {
                        try //- log the second phase error with some detail
                        {
                            var orignalExceptionPlusHandlingException =
                                new ExceptionHandlingException(new AggregateException(exceptionHandlingException, exception));

                            var exceptionMessages = new MessageExceptionInfo(orignalExceptionPlusHandlingException, message);

                            MMessageContext.Logger.Error("Error Handling Failed Message {@details}", exceptionMessages);

                            finalException = exceptionMessages.ToEnvironmentSpecificError();
                        }
                        catch (Exception lastChanceException) //- log a minimal error message of last resort
                        {
                            /* avoid use of PipelineExceptionMessages here as it theoretically could be the source of the error
                            * this goes raw to the caller so show don't show any exception details
                            * Serilog should swallow it's own internal errors so logging should be safe here
                            */

                            MMessageContext.Logger.Error(
                                $"Failed logging message {message.MessageId} with exception: {exception}",
                                lastChanceException);

                            var lastChanceExceptionMessageForCaller =
                                $"{MessageExceptionInfo.CodePrefixes.EXWHEX}: {MMessageContext.AppConfig.DefaultExceptionMessage}";

                            finalException = new Exception(lastChanceExceptionMessageForCaller);
                        }
                    }

                    throw finalException;
                }
            }

            void SetMessageMetaInContext(MessageLogEntry messageLogItem, ApiMessage message, (DateTime receivedAt, long ticks) timeStamp)
            {
                MMessageContext.AfterMessageAccepted.Set(
                    new MessageMeta
                    {
                        StartTicks = timeStamp.ticks,
                        ReceivedAt = timeStamp.receivedAt,
                        Schema = message.GetType().AssemblyQualifiedName,
                        RequestedBy = message.IdentityToken != null ? this.authenticator.Authenticate(message) : null,
                        MessageLogItem = messageLogItem
                    },
                    null);
            }
        }

        public static class Constants
        {
            public static readonly Guid ForceFailBeforeMessageCompletesAndFailErrorHandlerId = Guid.Parse("c7c607b3-fc47-4dbd-81da-8ef28c785a2f");

            public static readonly Guid ForceFailBeforeMessageCompletesId = Guid.Parse("006717f1-fe50-4d0b-b762-75b883ba4a65");
        }
    }
}