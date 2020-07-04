namespace Soap.NotificationServer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;

    public class NotificationServer
    {
        private readonly List<IServerChannelInfo> channels = new List<IServerChannelInfo>();

        private readonly Settings settings;

        public NotificationServer(Settings settings, List<INotificationChannelSettings> channelSettings)
        {
            this.settings = settings;
            channelSettings.ForEach(s => this.channels.Add(s.CreateChannel()));
        }

        public async Task Notify(Notification notification, params IUserChannelInfo[] selectedUserChannels)
        {
            foreach (var channelType in selectedUserChannels)
                switch (channelType)
                {
                    case { } email when email.Type == NotificationChannelTypes.Email:
                        await this.channels.SingleOrDefault(x => x.Type == NotificationChannelTypes.Email)?.Send(notification);
                        break;

                    case { } inmemory when inmemory.Type == NotificationChannelTypes.InMemory:
                        await this.channels.SingleOrDefault(x => x.Type == NotificationChannelTypes.InMemory)?.Send(notification);
                        break;
                    default:
                        return;
                }
        }

        public class Settings
        {
            public class Validator : AbstractValidator<Settings>
            {
                public Validator()
                {
                    RuleFor(x => x.ChannelSettings).NotNull();
                }
            }

            public List<INotificationChannelSettings> ChannelSettings { get; set; } = new List<INotificationChannelSettings>();

            public NotificationServer CreateServer() => new NotificationServer(this, ChannelSettings);
        }
    }
}