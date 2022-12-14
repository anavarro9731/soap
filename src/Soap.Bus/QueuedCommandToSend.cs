namespace Soap.Bus
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public class QueuedCommandToSend : IQueuedBusOperation
    {
        public DateTimeOffset? DeferUntil { get; set; }
        
        public ApiCommand CommandToSend { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }
}