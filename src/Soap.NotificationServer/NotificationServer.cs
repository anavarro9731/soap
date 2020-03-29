namespace Soap.NotificationServer
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;

    public class NotificationServer : INotificationServer
    {
        public NotificationServer(NotificationServerSettings settings, IMessageAggregator messageAggregator)
        {
            throw new System.NotImplementedException();
        }

        public void Notify(Notification notification, List<NotificationChannel> channels)
        {
            foreach (var notificationChannel in channels)
                switch (notificationChannel)
                {
                    case EmailNotification e: throw new KeyNotFoundException();
                }
        }
    }
}