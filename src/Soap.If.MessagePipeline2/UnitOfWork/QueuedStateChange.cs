namespace Soap.MessagePipeline.UnitOfWork
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;

    public class QueuedStateChange : IQueuedStateChange
    {
        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }
}