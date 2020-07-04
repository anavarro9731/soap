namespace Soap.NotificationServer.Channels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemoryChannel : IServerChannelInfo
    {
        public List<Notification> Notifications { get; set; } = new List<Notification>();

        public NotificationChannelTypes Type => NotificationChannelTypes.InMemory;

        public Task Send(Notification notification)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public class Settings : INotificationChannelSettings
        {
            public IServerChannelInfo CreateChannel() => new InMemoryChannel();
        }
    }
}