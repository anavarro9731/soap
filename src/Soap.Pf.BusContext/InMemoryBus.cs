namespace Soap.Bus
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public class InMemoryBus : IBusInternal
    {
        public Task Publish(ApiEvent publishEvent) => Task.CompletedTask;

        public Task Send(ApiCommand sendCommand) => Task.CompletedTask;
    }
}