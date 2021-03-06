namespace Soap.NotificationServer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using FluentValidation;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class NotificationServer
    {
        public readonly List<IServerChannelInfo> Channels = new List<IServerChannelInfo>();

        public NotificationServer(Settings settings, IMessageAggregator messageAggregator)
        {
            settings.ChannelSettings.ToList().ForEach(s => this.Channels.Add(s.CreateChannel(messageAggregator)));
        }

        public List<NotificationSent> NotificationsSent { get; set; } = new List<NotificationSent>();

        /// <summary>
        ///     Remember calls to Notify are always non-transactional so do not use in a transaction unless it is the only
        ///     operation.
        ///     Alternatively, create command for it and send a Notify message on the bus to a handler which notified in isolation
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="selectedUserChannels"></param>
        /// <returns></returns>
        public async Task Notify(
            Notification notification,
            bool failMessageIfFailedToSendNotification,
            params ChannelSpecificNotificationMeta[] channels)
        {
            /* you do want to record which channels messages are sent on, that is an important record.
             You want a record that a notification was sent, and to what channels. This is why an inmemory channel itself
             is not really the best design, you could use the inmemorychannel to record the message was sent and then 
             keep a separate list of where it was sent, or add that to the inmemory channel both design would be 
             messier than just saving that info in the server itself. like the bus messages. this shouldn't be a problem
             with resource allocation since the lifetime is only an api call. */

            if (new Notification.Validator().Validate(notification).IsValid)
            {
                if (channels.Any())
                {
                    var notificationSent = new NotificationSent
                    {
                        Notification = notification.Clone()
                    };

                    NotificationsSent.Add(notificationSent);

                    foreach (var channelMeta in channels)
                    {
                        switch (channelMeta)
                        {
                            case { } when channelMeta.Type == NotificationChannelTypes.Email:

                                var channel = GetChannelOrNull(NotificationChannelTypes.Email);
                                if (channel != null)
                                {
                                    await channel.Send(notification.Clone(), channelMeta);
                                    notificationSent.ChannelsSentTo.AddFlag(NotificationChannelTypes.Email);
                                }

                                break;
                        }
                    }
                }
            }
            else
            {
                if (failMessageIfFailedToSendNotification)
                {
                    new Notification.Validator().ValidateAndThrow(notification);
                }
            }

            IServerChannelInfo GetChannelOrNull(NotificationChannelTypes type)
            {
                var channel = this.Channels.SingleOrDefault(x => x.Type == type);

                Guard.Against(
                    channel == null && failMessageIfFailedToSendNotification,
                    $"Requested to send notification via {NotificationChannelTypes.Email.Value} channel but this channel is not availabe on the server");

                return channel;
            }
        }

        public class NotificationSent
        {
            public TypedEnumerationFlags<NotificationChannelTypes> ChannelsSentTo { get; set; } =
                new TypedEnumerationFlags<NotificationChannelTypes>();

            public Notification Notification { get; set; }
        }

        public class Settings
        {
            public INotificationChannelSettings[] ChannelSettings { get; set; } = new INotificationChannelSettings[0];

            public NotificationServer CreateServer(IMessageAggregator messageAggregator) =>
                new NotificationServer(this, messageAggregator);

            public class Validator : AbstractValidator<Settings>
            {
                public Validator()
                {
                    RuleFor(x => x.ChannelSettings).NotNull();
                }
            }
        }
    }
}
