namespace Soap.Context.Logging
{
    using System;
    using Soap.Context.Exceptions;
    using Soap.Interfaces.Messages;

    public abstract class SerilogApiCallLogEntry
    {
        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public bool IsCommand { get; set; }

        public bool IsEvent { get; set; }

        public ApiMessage Message { get; set; }

        public Guid MessageId { get; set; }

        public object ProfilingData { get; set; }

        public DateTime? SapiCompletedAt { get; set; }

        public DateTime? SapiReceivedAt { get; set; }

        public string Schema { get; set; }

        public bool Succeeded { get; set; }
        
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
        public FailedAttempt(FormattedExceptionInfo formattedExceptionInfoInfo)
        {
            Succeeded = false;
            FormattedExceptionInfo = formattedExceptionInfoInfo;
        }

        public FormattedExceptionInfo FormattedExceptionInfo { get; internal set; }
    }
}
