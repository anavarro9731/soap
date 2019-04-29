namespace Soap.Pf.EndpointClients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rebus.Activation;
    using Rebus.Bus;
    using Rebus.Config;
    using Rebus.Routing;
    using Rebus.Routing.TypeBased;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;

    public class BusApiClient : IDisposable, IBusContext
    {
        private readonly Action<RebusConfigurer> applyConfiguration;

        private readonly RebusConfigurer configurer;

        private readonly Action<IApiMessage> mutateMessageBeforeSending;

        private readonly Action onBusStarted;

        private readonly List<MsmqMessageRoute> routingDefinitions;

        private IBus bus;

        public bool IsOneWay { get; set; }

        public BusApiClient(
            IEnumerable<MsmqMessageRoute> routingDefinitions,
            Action<RebusConfigurer> applyConfiguration = null,
            RebusConfigurer configurer = null,
            Action<IApiMessage> mutateMessageBeforeSending = null,
            Action onBusStarted = null)
        {
            
                this.applyConfiguration = applyConfiguration ?? (r =>
                                                                                    {
                                                                                    r.Transport(a => a.UseMsmqAsOneWayClient());
                                                                                    this.IsOneWay = true;
                                                                                    });
                this.mutateMessageBeforeSending = mutateMessageBeforeSending ?? (m => { });
                this.configurer = configurer ?? Configure.With(new BuiltinHandlerActivator());
                this.onBusStarted = onBusStarted ?? (() => { });
                this.routingDefinitions = routingDefinitions?.ToList() ?? throw new ArgumentNullException(nameof(routingDefinitions));
            
        }

        public BusApiClient(MsmqMessageRoute route)
            : this(
                new[]
                {
                    route
                })
        {
        }

        ~BusApiClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //.. publish
        public async Task Publish(IApiEvent @event)
        {
            if (@event.MessageId == Guid.Empty) @event.MessageId = Guid.NewGuid();

            if (@event.TimeOfCreationAtOrigin.HasValue == false) @event.TimeOfCreationAtOrigin = DateTime.UtcNow;

            this.mutateMessageBeforeSending(@event);

            await this.bus.Publish(@event).ConfigureAwait(false);
        }

        //.. send command
        public async Task Send(IApiCommand command)
        {
            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            this.mutateMessageBeforeSending(command);

            await this.bus.Send(command).ConfigureAwait(false);
        }

        //.. send command to self
        public async Task SendLocal(IApiCommand command)
        {
                if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

                if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

                await this.bus.SendLocal(command).ConfigureAwait(false);
            
        }

        public Task Start()
        {
            StartIfNotStarted();

            return Task.CompletedTask;
        }

        private void Dispose(bool disposing)
        {
            //see rebus docs
            if (disposing)
            {
                this.bus?.Dispose();
            }
        }

        private void StartIfNotStarted()
        {
            if (this.bus != null) return;

            this.configurer.Logging(t => t.Serilog(Log.Logger)); //use serilog if root logger set

            void RouterConfig(StandardConfigurer<IRouter> r)
            {
                var typeBasedRouter = r.TypeBased();
                foreach (var routingDefinition in this.routingDefinitions)
                foreach (var routingDefinitionMessageType in routingDefinition.MessageTypes)
                    typeBasedRouter.Map(routingDefinitionMessageType, routingDefinition.MsmqEndpointAddress.ToString());
            }
            
            this.configurer.Routing(RouterConfig);

            this.applyConfiguration(this.configurer);

            this.bus = this.configurer.Start();

            this.onBusStarted();
        }
    }
}