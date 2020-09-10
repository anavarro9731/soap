namespace Soap.Bus
{
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class InMemoryBus : IBusInternal
    {
        private readonly Settings settings;

        private InMemoryBus(Settings settings)
        {
            this.settings = settings;
        }

        public Task Publish(ApiEvent publishEvent) => Task.CompletedTask;

        public Task Send(ApiCommand sendCommand) => Task.CompletedTask;

        public class Settings : IBusSettings
        {

            public IBus CreateBus(IMessageAggregator messageAggregator)
            {
                return new Bus(new InMemoryBus(this), this, messageAggregator);
            }

            public byte NumberOfApiMessageRetries { get; set; }
        }
    }
}