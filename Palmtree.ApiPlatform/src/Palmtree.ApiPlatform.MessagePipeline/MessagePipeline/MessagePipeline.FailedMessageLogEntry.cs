namespace Palmtree.ApiPlatform.MessagePipeline.MessagePipeline
{
    using Palmtree.ApiPlatform.Infrastructure.Models;

    public partial class MessagePipeline
    {
        public class FailedMessageLogEntry : MessageLogEntryBase
        {
            public PipelineExceptionMessages ExceptionMessages { get; set; }
        }
    }
}
