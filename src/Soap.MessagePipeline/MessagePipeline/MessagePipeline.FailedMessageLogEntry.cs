namespace Soap.MessagePipeline.MessagePipeline
{
    using Soap.MessagePipeline.Models;

    public partial class MessagePipeline
    {
        public class FailedMessageLogEntry : MessageLogEntryBase
        {
            public PipelineExceptionMessages ExceptionMessages { get; set; }
        }
    }
}
