namespace Soap.MessagePipeline
{
    using System;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Objects.Blended;

    public class GlobalErrorCodes : ErrorCode
    {
        public static readonly ErrorCode CouldNotFindHandlerForMessage = Create(
                Guid.Parse("6067c5cb-19c4-4458-b6e4-8e70999625e1"),
                "Could not map message to handler, as none exists for this message type.")
            .Op(m => m.IsGlobal = true);

        public static readonly ErrorCode ItemIsADifferentMessageWithTheSameId = Create(
                Guid.Parse("cc1841e5-d0b9-4781-9c50-137496d4e959"),
                "A message with this id has already been processed. This message will be discarded. If this is not a duplicate message please resend with a unique MessageId property value")
            .Op(m => m.IsGlobal = true);

        public static readonly ErrorCode MessageAlreadyFailedMaximumNumberOfTimes = Create(
                Guid.Parse("dd4f97b0-e659-4709-a7d2-881c59974fba"),
                "This message has been retried the maximum number of allowable times and failed to process successfully.")
            .Op(m => m.IsGlobal = true);

        public static readonly ErrorCode MessageHasAlreadyBeenProcessedSuccessfully = Create(
                Guid.Parse("52e3bc56-3c5c-4054-a222-57374e219090"),
                "This message has already been processed successfully")
            .Op(m => m.IsGlobal = true);
        
        public static readonly ErrorCode UnitOfWorkFailedUnitOfWorkRolledBack = Create(
                Guid.Parse("36312a82-ca04-4b09-978f-5bb9e2809c2d"),
                "Unit of work was rolled back successfully and cannot be processed again due to concurrency issues")
            .Op(m => m.IsGlobal = true);
    }
}