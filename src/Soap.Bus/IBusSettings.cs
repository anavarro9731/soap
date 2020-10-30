namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces;

    public interface IBusSettings
    {
        IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage);

        byte NumberOfApiMessageRetries { get; set; }

    }
}