namespace Soap.MessagePipeline.UnitOfWork
{
    using Soap.Interfaces.Messages;

    public class QueuedApiEvent : QueuedStateChange
    {
        public ApiEvent Event { get; set; }
    }
}