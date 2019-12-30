namespace Soap.If.MessagePipeline.Messages
{
    using Soap.If.Interfaces.Messages;

    public class QueuedApiEvent : QueuedStateChange
    {
        public ApiEvent Event { get; set; }
    }
}