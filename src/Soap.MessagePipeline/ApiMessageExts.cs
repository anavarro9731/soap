namespace Soap.MessagePipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using CircuitBoard;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public static class ApiMessageExtensions
    {
        
        internal static void Authenticate(this ApiMessage message, ContextWithMessage ctx, Action<IApiIdentity> outIdentity)
        {
            var identity = ctx.Authenticator.Authenticate(message);
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
                    ApiMessageValidationErrorCodes.ItemIsADifferentMessageWithTheSameId);

                Guard.Against(
                    HasAlreadyBeenProcessedSuccessfully(messageLogEntry),
                    ApiMessageValidationErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully);

                Guard.Against(
                    HasAlreadyFailedTheMaximumNumberOfTimesAllowed(messageLogEntry),
                    ApiMessageValidationErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes);

                message.RequiredNotNullOrThrow();
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
