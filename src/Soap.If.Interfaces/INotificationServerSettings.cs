namespace Soap.If.Interfaces
{
    using CircuitBoard.MessageAggregator;

    public interface INotificationServerSettings
    {
        INotifyUsers CreateServer(IMessageAggregator messageAggregator);
    }
}