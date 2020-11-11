namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;

    public interface IBusSettings
    {
        byte NumberOfApiMessageRetries { get; set; }

        IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage);
    }
}