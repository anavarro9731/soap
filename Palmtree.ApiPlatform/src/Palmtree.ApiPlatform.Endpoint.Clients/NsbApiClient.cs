namespace Palmtree.ApiPlatform.Endpoint.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Features;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Interfaces;

    public class NsbApiClient : IBusContext
    {
        private readonly string apiEndpointAddress;

        private readonly EndpointConfiguration endpointConfiguration;

        private readonly List<IRoutingDefinition> routingDefinitions;

        private IEndpointInstance endPointInstance;

        public NsbApiClient(string apiEndpointAddress)
        {
            if (string.IsNullOrWhiteSpace(apiEndpointAddress)) throw new ArgumentNullException(nameof(apiEndpointAddress));

            this.apiEndpointAddress = apiEndpointAddress;

            this.endpointConfiguration = new EndpointConfiguration(nameof(NsbApiClient));

            ConfigureSendOnly();
        }

        public NsbApiClient(IEnumerable<IRoutingDefinition> routingDefinitions)
        {
            this.routingDefinitions = routingDefinitions?.ToList() ?? throw new ArgumentNullException(nameof(routingDefinitions));

            this.endpointConfiguration = new EndpointConfiguration(nameof(NsbApiClient));

            ConfigureSendOnly();
        }

        public void ConfigureConventions(Func<Type, bool> definingCommandAs, Func<Type, bool> definingEventsAs)
        {
            var conventions = this.endpointConfiguration.Conventions();

            conventions.DefiningCommandsAs(definingCommandAs);

            conventions.DefiningEventsAs(definingEventsAs);
        }

        public void ConfigureScannedAssemblies(IEnumerable<string> assemblyFileNamesToExclude)
        {
            this.endpointConfiguration.ExcludeAssemblies(assemblyFileNamesToExclude.ToArray());
        }

        public async Task SendCommand(IApiCommand command)
        {
            await ((IBusContext)this).Send(new SendCommandOperation(command)).ConfigureAwait(false);
        }

        public async Task SendOutsideTransaction(ISendCommandOperation sendCommand)
        {
            await SendCommand(sendCommand.Command).ConfigureAwait(false);
        }

        public async Task Start()
        {
            await StartIfNotStarted().ConfigureAwait(false);
        }

        async Task IBusContext.Publish(IPublishEventOperation publishEvent)
        {
            await StartIfNotStarted().ConfigureAwait(false);

            await this.endPointInstance.Publish(publishEvent.Event).ConfigureAwait(false);
        }

        async Task IBusContext.Send(ISendCommandOperation sendCommand)
        {
            await StartIfNotStarted().ConfigureAwait(false);

            var command = sendCommand.Command;

            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            if (this.apiEndpointAddress != null)
            {
                await this.endPointInstance.Send(this.apiEndpointAddress, command).ConfigureAwait(false);
            }
            else
            {
                var destination = this.routingDefinitions?.FirstOrDefault(rd => rd.CanRoute(command));

                if (destination == null) throw new Exception($"No message routing definition exists for {command.GetType()}.");

                await this.endPointInstance.Send($"{destination.EndpointName}@{destination.EndpointMachine}", command).ConfigureAwait(false);
            }
        }

        async Task IBusContext.SendLocal(ISendCommandOperation sendCommand)
        {
            await StartIfNotStarted().ConfigureAwait(false);

            var command = sendCommand.Command;

            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            await this.endPointInstance.SendLocal(sendCommand.Command).ConfigureAwait(false);
        }

        private void ConfigureSendOnly()
        {
            this.endpointConfiguration.SendOnly();

            //required when using sendonly without persistence
            this.endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();

            this.endpointConfiguration.UseSerialization<JsonSerializer>();

            this.endpointConfiguration.SendFailedMessagesTo($"{nameof(NsbApiClient)}.error");
        }

        private async Task StartIfNotStarted()
        {
            if (this.endPointInstance != null) return;

            this.endPointInstance = await Endpoint.Start(this.endpointConfiguration).ConfigureAwait(false);
        }
    }
}
