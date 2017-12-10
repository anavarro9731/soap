namespace Soap.Pf.EndpointClients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rebus.Activation;
    using Rebus.Bus;
    using Rebus.Config;
    using Rebus.Logging;
    using Rebus.Transport;
    using Serilog;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Routing;

    public abstract class MsmqApiClient : IDisposable
    {
        private readonly BuiltinHandlerActivator rebusActivator = new BuiltinHandlerActivator();

        private readonly List<MessageRoute_Msmq> routingDefinitions;

        private IBus bus;

        protected MsmqApiClient(IEnumerable<MessageRoute_Msmq> routingDefinitions)
        {
            this.routingDefinitions = routingDefinitions?.ToList() ?? throw new ArgumentNullException(nameof(routingDefinitions));
        }

        ~MsmqApiClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task Start()
        {
            StartIfNotStarted();

            return Task.CompletedTask;
        }

        protected abstract RebusConfigurer ConfigureRebus(RebusConfigurer config);

        protected abstract void ConfigureRebusTransport(StandardConfigurer<ITransport> configurer);

        protected virtual void MutateApiMessageBeforeSend(IApiMessage message)
        {
        }

        protected virtual void OnBusStarted()
        {
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.rebusActivator?.Dispose();
                this.bus?.Dispose();
            }
        }

        //.. publish
        private async Task Publish(IApiEvent @event)
        {
            if (@event.MessageId == Guid.Empty) @event.MessageId = Guid.NewGuid();

            if (@event.TimeOfCreationAtOrigin.HasValue == false) @event.TimeOfCreationAtOrigin = DateTime.UtcNow;

            MutateApiMessageBeforeSend(@event);

            await this.bus.Publish(@event).ConfigureAwait(false);
        }

        //.. Send Command
        private async Task SendCommand(IApiCommand command)
        {
            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            MutateApiMessageBeforeSend(command);

            await this.bus.Send(command).ConfigureAwait(false);
        }

        //.. Send command to self
        private async Task SendLocal(IApiCommand command)
        {
            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            if (command.TimeOfCreationAtOrigin.HasValue == false) command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            await this.bus.SendLocal(command).ConfigureAwait(false);
        }

        private void StartIfNotStarted()
        {
            if (this.bus != null) return;

            var config = Configure.With(this.rebusActivator)
                                  .Logging(t => t.Serilog(Log.Logger)) //use serilog if root logger set
                                  .Transport(ConfigureRebusTransport)
                                  .Routing(r => r.Register(ctx => new CustomRebusRouter(this.routingDefinitions, ctx.Get<IRebusLoggerFactory>())));

            ConfigureRebus(config);

            this.bus = config.Start();

            OnBusStarted();
        }
    }
}