namespace Soap.Bus
{
    using CircuitBoard.Messages;

    public interface IQueuedBusOperation : IMessage
    {
        bool Committed { get; set; }
    }
}