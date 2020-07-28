namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using FluentValidation;

    public class BusSettings
    {
        public byte NumberOfApiMessageRetries;

        public string QueueConnectionString;

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
        public static IBusInternal CreateBus(this BusSettings busSettings, IMessageAggregator messageAggregator) =>
            new AzureBus(messageAggregator, busSettings);
    }
}