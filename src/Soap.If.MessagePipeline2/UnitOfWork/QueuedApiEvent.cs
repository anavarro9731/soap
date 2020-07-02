namespace Soap.MessagePipeline.UnitOfWork
{
    using Soap.Interfaces;

    public class QueuedApiEvent : QueuedStateChange
    {
        public ApiEvent Event { get; set; }
    }
}