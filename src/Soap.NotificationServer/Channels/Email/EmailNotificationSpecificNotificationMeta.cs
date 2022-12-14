namespace Soap.NotificationServer.Channels.Email
{
    public class EmailNotificationSpecificNotificationMeta : ChannelSpecificNotificationMeta
    {
        public EmailNotificationSpecificNotificationMeta(string recipient, string fromAddress = null)
            : base(recipient, NotificationChannelTypes.Email)
        {
            FromAddress = fromAddress;
        }

        public string FromAddress { get; }
    }
}
