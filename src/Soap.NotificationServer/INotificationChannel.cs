namespace Soap.NotificationServer
{
    public interface INotificationChannel
    {
        NotificationChannelTypes Type { get; }
    }
}
