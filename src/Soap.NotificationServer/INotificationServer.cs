namespace Soap.NotificationServer
{
    using System.Collections.Generic;

    public interface INotificationServer
    {
        void Notify(Notification notification, List<NotificationChannel> channels);
    }
}