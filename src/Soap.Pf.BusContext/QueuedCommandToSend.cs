namespace Soap.Bus
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;

    public class QueuedCommandToSend : IQueuedBusOperation
    {
        public ApiCommand CommandToSend { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }
}