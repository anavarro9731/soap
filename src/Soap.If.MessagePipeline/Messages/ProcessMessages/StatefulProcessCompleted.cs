namespace Soap.If.MessagePipeline.Messages.ProcessMessages
{
    using Soap.If.MessagePipeline.Models.Aggregates;

    public class StatefulProcessCompleted : ProcessCompleted
    {
        public StatefulProcessCompleted(string processType, string username, ProcessState finalState)
            : base(processType, username)
        {
            FinalState = finalState;
        }

        public ProcessState FinalState { get; set; }
    }
}