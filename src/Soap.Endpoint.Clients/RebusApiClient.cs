namespace Soap.Endpoint.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rebus.Activation;
    using Rebus.Bus;
    using Rebus.Config;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class RebusApiClient : IBusContext
    {
        private readonly BuiltinHandlerActivator activator = new BuiltinHandlerActivator();

        private readonly string apiEndpointAddress;

        private readonly List<IRoutingDefinition> routingDefinitions;

        private IBus bus;

        public RebusApiClient(string apiEndpointAddress)
        {
            if (string.IsNullOrWhiteSpace(apiEndpointAddress)) throw new ArgumentNullException(nameof(apiEndpointAddress));

            this.apiEndpointAddress = apiEndpointAddress;
        }

        public RebusApiClient(IEnumerable<IRoutingDefinition> routingDefinitions)
        {
            this.routingDefinitions = routingDefinitions?.ToList() ?? throw new ArgumentNullException(nameof(routingDefinitions));
        }

        ~RebusApiClient()
        {
            this.activator?.Dispose();
        }

        public async Task SendCommand(IApiCommand command)
        {
            await ((IBusContext)this).Send(new SendCommandOperation(command)).ConfigureAwait(false);
        }

        public async Task SendOutsideTransaction(ISendCommandOperation sendCommand)
        {
            await SendCommand(sendCommand.Command).ConfigureAwait(false);
        }

        public Task Start()
        {
            StartIfNotStarted();
            return Task.CompletedTask;
        }

        async Task IBusContext.Publish(IPublishEventOperation publishEvent)
        {
            StartIfNotStarted();

            await this.bus.Publish(publishEvent.Event).ConfigureAwait(false);
        }

        async Task IBusContext.Send(ISendCommandOperation sendCommand)
        {
            StartIfNotStarted();

            var command = sendCommand.Command;

            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            if (this.apiEndpointAddress != null)
            {
                await this.bus.Advanced.Routing.Send(this.apiEndpointAddress, command).ConfigureAwait(false);
            }
            else
            {
                var destination = this.routingDefinitions?.FirstOrDefault(rd => rd.CanRoute(command));

                if (destination == null) throw new Exception($"No message routing definition exists for {command.GetType()}.");

                await this.bus.Advanced.Routing.Send($"{destination.EndpointName}@{destination.EndpointMachine}", command).ConfigureAwait(false);
            }
        }

        async Task IBusContext.SendLocal(ISendCommandOperation sendCommand)
        {
            StartIfNotStarted();

            var command = sendCommand.Command;

            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            await this.bus.SendLocal(sendCommand.Command).ConfigureAwait(false);
        }

        private void StartIfNotStarted()
        {
            if (this.bus != null) return;

            this.bus = Configure.With(this.activator)
                                .Transport(t => t.UseMsmqAsOneWayClient())
                                .Logging(t => t.Serilog(Log.Logger)) //use serilog if root logger set
                                .Start();
        }
    }
}