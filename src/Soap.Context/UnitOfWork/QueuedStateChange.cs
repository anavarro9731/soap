namespace Soap.Context.UnitOfWork
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using Soap.Bus;

    public class QueuedStateChange : IQueuedStateChange
    {
        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }
}