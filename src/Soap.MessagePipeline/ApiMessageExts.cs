namespace Soap.MessagePipeline
{
    using System;
    using Newtonsoft.Json;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class ApiMessageExtensions
    {
        internal static void Authenticate(this ApiMessage message, ContextWithMessage ctx, Action<IApiIdentity> outIdentity)
        {
            var identity = message.Headers.GetIdentityToken() != null ? ctx.Authenticator.Authenticate(message) : null;
            outIdentity(identity);
        }

        internal static string GetSchema(this ApiMessage m) => m.GetType().ToShortAssemblyTypeName();

        internal static void ValidateOrThrow(this ApiMessage message, ContextWithMessageLogEntry context)
        {
            {
                var messageLogEntry = context.MessageLogEntry;

                message.ValidateIncomingMessageHeaders();

                Guard.Against(
                    IsADifferentMessageButWithTheSameId(messageLogEntry, message),
                    GlobalErrorCodes.ItemIsADifferentMessageWithTheSameId);

                Guard.Against(
                    HasAlreadyBeenProcessedSuccessfully(messageLogEntry),
                    GlobalErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully);

                Guard.Against(
                    HasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogEntry),
                    GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes);

                context.Validate(message);
            }

            bool HasAlreadyBeenProcessedSuccessfully(MessageLogEntry messageLogEntry) =>
                //- safeguard, cannot think of a reason it would happen 
                messageLogEntry.UnitOfWork != null && messageLogEntry.ProcessingComplete;

            bool HasAlreadyFailedTheMaximumNumberOfTimesAllowed(MessageLogEntry messageLogEntry) =>
                //should never happen, unless message broker/bus it configured to retry message more times
                //than the pipeline is configured to allow
                messageLogEntry.Attempts.Count > context.Bus.MaximumNumberOfRetries;

            bool IsADifferentMessageButWithTheSameId(MessageLogEntry messageLogEntry, ApiMessage message)
            {
                var messageAsJson = message.ToJson(SerialiserIds.ApiBusMessage);
                var hashMatches = messageAsJson.Verify(messageLogEntry.MessageHash);
                return !hashMatches;
            }
        }
    }
}
