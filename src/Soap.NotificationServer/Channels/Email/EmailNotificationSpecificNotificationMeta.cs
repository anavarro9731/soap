namespace Soap.NotificationServer.Channels
{
    using Soap.Interfaces;

    public class EmailNotificationSpecificNotificationMeta : ChannelSpecificNotificationMeta
    {
        public EmailNotificationSpecificNotificationMeta(string recipient, string fromAddress)
            : base(recipient, NotificationChannelTypes.Email)
        {
            FromAddress = fromAddress;
        }

        public string FromAddress { get; }
    }
}
