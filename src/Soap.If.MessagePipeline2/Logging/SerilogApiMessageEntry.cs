namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;

    public abstract class SerilogApiMessageEntry
    {
        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public bool IsCommand { get; set; }

        public bool IsEvent { get; set; }

        public ApiMessage Message { get; set; }

        public bool IsQuery { get; set; }

        public Guid MessageId { get; set; }

        public object ProfilingData { get; set; }

        public DateTime? SapiCompletedAt { get; set; }

        public DateTime? SapiReceivedAt { get; set; }

        public string Schema { get; set; }

        public bool Succeeded { get; internal set; }

        public string UserName { get; set; }
    }

    public class SuccessfulAttempt : SerilogApiMessageEntry
    {
        public SuccessfulAttempt()
        {
            Succeeded = true;
        }
    }

    public class FailedAttempt : SerilogApiMessageEntry
    {
        public FailedAttempt(FormattedExceptionInfo exceptionInfo)
        {
            Succeeded = false;
            ExceptionFormatteds = exceptionInfo;
        }

        public FormattedExceptionInfo ExceptionFormatteds { get; internal set; }

    }
}