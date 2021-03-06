namespace Soap.MessagePipeline
{
    using System;
    using Soap.Context;
    using Soap.Utility;

    public class ApiMessageValidationErrorCodes : ErrorCode
    {
        /* Error Codes only need to be mapped if there is front-end logic that might depend on them
         otherwise the default error handling logic will do the job of returning the error message but without a specific code. */

        public static readonly ErrorCode ItemIsADifferentMessageWithTheSameId = Create(
            Guid.Parse("cc1841e5-d0b9-4781-9c50-137496d4e959"),
            "A message with this id has already been processed. This message will be discarded. If this is not a duplicate message please resend with a unique MessageId property value");

        public static readonly ErrorCode MessageAlreadyFailedMaximumNumberOfTimes = Create(
            Guid.Parse("dd4f97b0-e659-4709-a7d2-881c59974fba"),
            "This message has been retried the maximum number of allowable times and failed to process successfully.");

        public static readonly ErrorCode MessageHasAlreadyBeenProcessedSuccessfully = Create(
            Guid.Parse("52e3bc56-3c5c-4054-a222-57374e219090"),
            "This message has already been processed successfully");

    }

}
