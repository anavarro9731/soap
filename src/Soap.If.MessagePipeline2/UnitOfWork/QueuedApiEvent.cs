namespace Soap.If.MessagePipeline.UnitOfWork
{
    using Soap.If.Interfaces.Messages;

    public class QueuedApiEvent : QueuedStateChange
    {
        public ApiEvent Event { get; set; }
    }
}