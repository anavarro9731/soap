namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Messages;

    /// <summary>
    ///     this class queues any actions the user performs during a session (message) which
    ///     conceptually alter external state (i/o bound operations)
    /// </summary>
    public class MessageBus
    {
        private readonly IBusContext busContext;

        internal MessageBus(IBusContext busContext)
        {
            this.busContext = busContext;
            //- only access from MessageContext
            //- keep all methods static
        }

        public static Task ExecuteChanges()
        {
            return QueuedStateChanges.CommitChanges();
        }

        public static void QueueStateChange(IQueuedStateChange queuedStateChange)
        {
            QueuedStateChanges.QueueChange(queuedStateChange);
        }

        public void PublishEvent(ApiEvent @event)
        {
            @event.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiEvent
                {
                    CommitClosure = () => this.busContext.Publish(@event),
                    Event = @event
                });
        }

        public void SendCommand(ApiCommand command)
        {
            command.TimeOfCreationAtOrigin = DateTime.UtcNow;

            QueueStateChange(
                new QueuedApiCommand
                {
                    CommitClosure = () => this.busContext.Send(command),
                    Command = command
                });
        }

        //TODO reply
    }
}