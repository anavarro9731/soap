namespace Soap.NotificationServer
{
    using System.Threading.Tasks;

    public interface INotificationChannel
    {
        NotificationChannelTypes Type { get; }
    }

    public interface IServerChannelInfo : INotificationChannel
    {
        Task Send(Notification notification);
    }

    public interface IUserChannelInfo : INotificationChannel
    {
    }

    public interface INotificationChannelSettings
    {
        IServerChannelInfo CreateChannel();
    }
}