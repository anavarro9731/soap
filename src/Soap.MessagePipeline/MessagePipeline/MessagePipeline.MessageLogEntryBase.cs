namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public partial class MessagePipeline
    {
        public class MessageLogEntryBase
        {
            public string ApplicationName { get; set; }

            public string EnvironmentName { get; set; }

            public bool IsCommand { get; set; }

            public bool IsEvent { get; set; }

            public bool IsQuery { get; set; }

            public IApiMessage Message { get; set; }

            public Guid MessageId { get; set; }

            public object ProfilingData { get; set; }

            public DateTime SapiCompletedAt { get; set; }

            public DateTime SapiReceivedAt { get; set; }

            public string Schema { get; set; }

            public string UserName { get; set; }
        }
    }
}
