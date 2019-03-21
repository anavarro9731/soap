namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Messages;
    using Soap.If.Utility.PureFunctions;

    /// <summary>
    ///     this class queues any actions the user performs during a session (message) which
    ///     conceptually alter external state (i/o bound operations)
    /// </summary>
    public class UnitOfWork 
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly QueuedStateChanger stateChanger;

        private IBusContext busContext;

        public UnitOfWork(QueuedStateChanger stateChanger, IMessageAggregator messageAggregator, IBusContext busContext)
        {
            this.stateChanger = stateChanger;
            this.messageAggregator = messageAggregator;
            this.busContext = busContext;
            TransactionId = Guid.NewGuid();
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
                    CommitClosure = () => this.busContext.Publish(@event),
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
                    CommitClosure = () => this.busContext.Send(command),
                    Command = command
                });
        }

        public void SendLocal(IApiCommand command)
        {
            command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiCommand
                {
                    CommitClosure = () => this.busContext.SendLocal(command),
                    Command = command
                });
        }

        //reply

    }
}