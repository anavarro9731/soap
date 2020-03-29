namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using FluentValidation;

    public class BusSettings
    {
        public string QueueConnectionString;

        public byte NumberOfApiMessageRetries;

        public class Validator : AbstractValidator<BusSettings>
        {
            public Validator()
            {
                RuleFor(x => x.QueueConnectionString).NotEmpty();
                RuleFor(x => x.NumberOfApiMessageRetries).GreaterThanOrEqualTo(x => 0);

            }
        }
    }

    public static class BusSettingsExtensions
    {
        public static IBus CreateBus(this BusSettings busSettings, IMessageAggregator messageAggregator) =>
            new Bus(messageAggregator, busSettings);
    }
}