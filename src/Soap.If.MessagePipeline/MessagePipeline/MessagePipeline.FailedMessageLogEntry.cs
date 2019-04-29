namespace Soap.If.MessagePipeline.MessagePipeline
{
    using Soap.If.MessagePipeline.Models;

    public partial class MessagePipeline
    {
        public class FailedMessageLogEntry : MessageLogEntryBase
        {
            public PipelineExceptionMessages ExceptionMessages { get; set; }
        }
    }
}