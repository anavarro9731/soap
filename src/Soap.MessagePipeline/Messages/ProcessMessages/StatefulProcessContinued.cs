namespace Soap.MessagePipeline.Messages.ProcessMessages
{
    using Soap.MessagePipeline.Models.Aggregates;

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
