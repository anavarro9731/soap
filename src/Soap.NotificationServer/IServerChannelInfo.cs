namespace Soap.NotificationServer
{
    using System.Threading.Tasks;
    using Soap.Interfaces;

    public interface IServerChannelInfo : INotificationChannel
    {
        Task Send(Notification notification);
    }
}
