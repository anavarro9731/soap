namespace Soap.Bus
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;

    public class QueuedPublishEvent : IQueuedBusOperation
    {
        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public ApiEvent EventToSend { get; set; }
    }
}