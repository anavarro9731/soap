namespace Soap.NotificationServer
{
    public abstract class ChannelSpecificNotificationMeta : INotificationChannel
    {
        protected ChannelSpecificNotificationMeta(string recipient, NotificationChannelTypes type)
        {
            Recipient = recipient;
            Type = type;
        }

        //* only one per notification otherwise its a new notification
        public string Recipient { get; set; }

        public NotificationChannelTypes Type { get; }
    }
}
