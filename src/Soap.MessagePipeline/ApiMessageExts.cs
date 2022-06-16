namespace Soap.MessagePipeline
{
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class ApiMessageExtensions
    {
        internal static string GetSchema(this ApiMessage m) => m.GetType().ToShortAssemblyTypeName();

        internal static void ValidateOrThrow(this ApiMessage message, ContextWithMessageLogEntry context)
        {
            {
                var messageLogEntry = context.MessageLogEntry;

                message.ValidateIncomingMessageHeaders();

                Guard.Against(
                    IsADifferentMessageButWithTheSameId(messageLogEntry),
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

            static bool HasAlreadyBeenProcessedSuccessfully(MessageLogEntry messageLogEntry) =>
                //- safeguard, cannot think of a reason it would happen 
                messageLogEntry.ProcessingComplete;

            bool HasAlreadyFailedTheMaximumNumberOfTimesAllowed(MessageLogEntry messageLogEntry) =>
                //should never happen, unless message broker/bus it configured to retry message more times
                //than the pipeline is configured to allow
                messageLogEntry.Attempts.Count > context.Bus.MaximumNumberOfRetries;

            
            bool IsADifferentMessageButWithTheSameId(MessageLogEntry messageLogEntry)
            { //* we know its the same id because we have already loaded the MessageLogEntry for this message

                /* if it was hydrated from a blob the contents will have changed vs what was stored in the log entry
                 so we always use the perimeter message to validate against. this is fine because even though we dont
                 have the contents to check for a match we have the blob id, and as long as that hasn't changed we 
                 know we have the same contents */

                var messageToVerifyAgainst = context.MessageLogEntry.SkeletonOnly ? message.Clone()/*don't affect msg about to go through pipeline*/.ClearAllPublicPropertyValuesExceptHeaders() : message;
                
                var messageAsJson = messageToVerifyAgainst.ToJson(SerialiserIds.ApiBusMessage);

                var hashMatches = messageAsJson.Verify(messageLogEntry.MessageHash);
               
                return !hashMatches;
            }
        }
    }
}
