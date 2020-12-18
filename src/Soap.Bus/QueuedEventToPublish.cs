namespace Soap.Bus
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class QueuedEventToPublish : IQueuedBusOperation
    {
        public Func<Task> CommitClosure { get; set; }

        public EnumerationFlags EventVisibility { get; set; }
        
        public bool Committed { get; set; }

        public ApiEvent EventToPublish { get; set; }
    }
}
