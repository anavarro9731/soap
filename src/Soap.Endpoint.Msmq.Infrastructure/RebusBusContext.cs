namespace Soap.Pf.MsmqEndpointBase
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Rebus.Autofac;
    using Rebus.Backoff;
    using Rebus.Bus;
    using Rebus.Config;
    using Rebus.Retry.Simple;
    using Rebus.Routing.TypeBased;
    using Rebus.TransactionScopes;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class RebusBusContext : IBusContext
    {
        private readonly IBus bus;

        private RebusBusContext(IBus bus)
        {
            this.bus = bus;
        }

        public static IBusContext Create<TSampleDomainMessage>(IApplicationConfig appConfig, IContainer autofacContainer, bool logPipeline)
            where TSampleDomainMessage : IApiMessage
        {
            var rebus = Configure.With(new AutofacContainerAdapter(autofacContainer))
                                 .Logging(l => l.Serilog(Log.Logger))
                                 .Transport(t => t.UseMsmq(appConfig.ApiEndpointSettings.MsmqEndpointAddress))
                                 .Routing(
                                     r => r.TypeBased()
                                           .MapAssemblyOf<TSampleDomainMessage>(appConfig.ApiEndpointSettings.MsmqEndpointAddress)
                                           .MapAssemblyOf<IMessageFailedAllRetries>(appConfig.ApiEndpointSettings.MsmqEndpointAddress))
                                 .Options(
                                     o =>
                                         {
                                         //not sure if this is necessary with our own
                                         //depends if this causes queue operations to enlist
                                         //but according to docs i don't think it does
                                         //need to check this and the messageconstraints.enforce
                                         //if it doesnt to ensure right behaviour
                                         //not used if swapped?
                                         o.HandleMessagesInsideTransactionScope();

                                         o.SimpleRetryStrategy(
                                             $"{appConfig.ApiEndpointSettings.MsmqEndpointName}.error",
                                             appConfig.NumberOfApiMessageRetries + 1,
                                             false);

                                         o.SetBackoffTimes(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));

                                         o.SetNumberOfWorkers(1);
                                         o.SetMaxParallelism(1);

                                         if (logPipeline) o.LogPipeline();
                                         })
                                 .Start();

            var builder = new ContainerBuilder();

            var busContext = new RebusBusContext(rebus);
            //hotswap tx, ms msg storage
            builder.RegisterInstance(busContext).As<IBusContext>();

            builder.Update(autofacContainer);

            return busContext;
        }

        public async Task Publish(IPublishEventOperation publishEvent)
        {
            await this.bus.Publish(publishEvent.Event).ConfigureAwait(false);
        }

        public async Task Send(ISendCommandOperation sendCommand)
        {
            await this.bus.Send(sendCommand.Command).ConfigureAwait(false);
        }

        public async Task SendLocal(ISendCommandOperation sendCommand)
        {
            await this.bus.SendLocal(sendCommand.Command).ConfigureAwait(false);
        }
    }
}