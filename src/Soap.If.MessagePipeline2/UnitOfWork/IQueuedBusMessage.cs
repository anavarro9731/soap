namespace Soap.MessagePipeline.UnitOfWork
{
    using CircuitBoard.Messages;

    public interface IQueuedBusMessage : IQueuedStateChange
    {
    }
}