namespace Soap.NotificationServer
{
    using CircuitBoard.MessageAggregator;

    public interface INotificationChannelSettings
    {
        IServerChannelInfo CreateChannel(IMessageAggregator messageAggregator);
    }
}
