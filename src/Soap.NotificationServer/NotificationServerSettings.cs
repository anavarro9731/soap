namespace Soap.NotificationServer
{
    using CircuitBoard.MessageAggregator;
    using FluentValidation;

    public class NotificationServerSettings
    {
        public string EmailApiConnectionString;

        public class Validator : AbstractValidator<NotificationServerSettings>
        {
            public Validator()
            {
                RuleFor(x => x.EmailApiConnectionString).NotEmpty();
            }
        }
    }

    public static class NotificationServerSettingsExtensions
    {

        public static NotificationServer CreateNotificationServer(this NotificationServerSettings settings, IMessageAggregator messageAggregator)
        {
            return new NotificationServer(settings, messageAggregator);
        }
    }
}