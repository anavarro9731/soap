namespace Soap.NotificationServer
{
    public interface INotificationChannelSettings
    {
        IServerChannelInfo CreateChannel();
    }
}