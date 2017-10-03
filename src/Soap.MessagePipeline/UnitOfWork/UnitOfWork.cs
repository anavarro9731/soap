namespace Soap.MessagePipeline.UnitOfWork
{
    using System;
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Messages;
    using Soap.Utility.PureFunctions;

    /// <summary>
    ///     this class queues any actions the user performs during a session (message) which
    ///     conceptually alter external state (i/o bound operations)
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly QueuedStateChanger stateChanger;

        private IBusContext busContext;

        public UnitOfWork(QueuedStateChanger stateChanger, IMessageAggregator messageAggregator)
        {
            this.stateChanger = stateChanger;
            this.messageAggregator = messageAggregator;
            TransactionId = Guid.NewGuid();
        }

        public UnitOfWork(QueuedStateChanger stateChanger, IBusContext busContext, IMessageAggregator messageAggregator)
            : this(stateChanger, messageAggregator)
        {
            this.busContext = busContext;
        }

        public Guid TransactionId { get; }

        public async Task ExecuteChanges()
        {
            Guard.Against(this.busContext == null, "IBusContext not set, set via constructor or .SetBusContext() method");

            await this.stateChanger.CommitChanges().ConfigureAwait(false);
        }

        public void PublishEvent(IApiEvent @event)
        {
            @event.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiEvent
                {
                    CommitClosure = () => this.messageAggregator.CollectAndForward(new PublishEventOperation(@event)).To(this.busContext.Publish),
                    Event = @event
                });
        }

        public void QueueStateChange(IQueuedStateChange queuedStateChange)
        {
            this.stateChanger.QueueChange(queuedStateChange);
        }

        public void SendCommand(IApiCommand command)
        {
            command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiCommand
                {
                    CommitClosure = () => this.messageAggregator.CollectAndForward(new SendCommandOperation(command)).To(this.busContext.Send),
                    Command = command
                });
        }

        public void SendCommandToSelf(IApiCommand command)
        {
            command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiCommand
                {
                    CommitClosure = () => this.messageAggregator.CollectAndForward(new SendCommandOperation(command)).To(this.busContext.SendLocal),
                    Command = command
                });
        }

        public void SetBusContext(IBusContext busContext)
        {
            this.busContext = busContext;
        }
    }
}
