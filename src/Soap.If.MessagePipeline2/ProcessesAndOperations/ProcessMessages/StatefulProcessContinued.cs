namespace Soap.If.MessagePipeline.Messages.ProcessMessages
{
    using Soap.If.MessagePipeline.Models.Aggregates;

    public class StatefulProcessContinued : ProcessEvent
    {
        public StatefulProcessContinued(string processType, string username, ProcessState initialState)
            : base(processType, username)
        {
            InitialState = initialState;
        }

        public ProcessState InitialState { get; set; }
    }
}