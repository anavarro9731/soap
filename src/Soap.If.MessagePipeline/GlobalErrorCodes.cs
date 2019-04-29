namespace Soap.If.MessagePipeline
{
    using System;
    using Soap.If.Interfaces;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class GlobalErrorCodes : ErrorCode
    {
        public static readonly GlobalErrorCodes MessageAlreadyFailedMaximumNumberOfTimes = Create<GlobalErrorCodes>(
            Guid.Parse("dd4f97b0-e659-4709-a7d2-881c59974fba"),
            "This message has been retried the maximum number of allowable times and failed to process successfully.").Op(m => m.IsGlobal = true);

        public static readonly GlobalErrorCodes MessageHasAlreadyBeenProcessedSuccessfully = 
            Create<GlobalErrorCodes>(Guid.Parse("52e3bc56-3c5c-4054-a222-57374e219090"),
                "This message has already been processed successfully").Op(m => m.IsGlobal = true);

        public static readonly GlobalErrorCodes ItemIsADifferentMessageWithTheSameId =
            Create<GlobalErrorCodes>(Guid.Parse("cc1841e5-d0b9-4781-9c50-137496d4e959"),
                "A message with this id has already been processed. This message will be discarded. If this is not a duplicate message please resend with a unique MessageId property value")
                .Op(m => m.IsGlobal = true);

    }


}