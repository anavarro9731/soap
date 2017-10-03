namespace Soap.MessagePipeline.Messages
{
    using System;
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

    public class QueuedStateChange : IQueuedStateChange
    {
        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }
}
