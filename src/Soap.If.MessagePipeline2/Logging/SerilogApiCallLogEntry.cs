namespace Soap.MessagePipeline.Logging
{
    using System;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;

    public abstract class SerilogApiCallLogEntry
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

    public class SuccessfulAttempt : SerilogApiCallLogEntry
    {
        public SuccessfulAttempt()
        {
            Succeeded = true;
        }
    }

    public class FailedAttempt : SerilogApiCallLogEntry
    {
        public FailedAttempt(FormattedExceptionInfo exceptionInfo)
        {
            Succeeded = false;
            ExceptionFormatteds = exceptionInfo;
        }

        public FormattedExceptionInfo ExceptionFormatteds { get; internal set; }

    }
}