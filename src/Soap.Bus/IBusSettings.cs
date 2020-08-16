namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;

    public interface IBusSettings
    {
        IBus CreateBus(IMessageAggregator messageAggregator);

        byte NumberOfApiMessageRetries { get; set; }
    }
}