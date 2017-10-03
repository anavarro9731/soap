namespace Soap.MessagePipeline.Messages.ProcessMessages
{
    using Soap.MessagePipeline.Models.Aggregates;

    public class StatefulProcessCompleted : ProcessCompleted
    {
        public StatefulProcessCompleted(string processType, string username)
            : base(processType, username)
        {
        }

        public ProcessState FinalState { get; set; }
    }
}
