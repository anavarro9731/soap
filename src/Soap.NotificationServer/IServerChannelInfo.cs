namespace Soap.NotificationServer
{
    using System.Threading.Tasks;

    public interface IServerChannelInfo : INotificationChannel
    {
        Task Send(Notification notification);
    }
}