namespace Soap.If.MessagePipeline.MessagePipeline
{
    public partial class MessagePipeline
    {
        public class SuccessfulMessageLogEntry : MessageLogEntryBase
        {
            public object Result { get; set; }
        }
    }
}