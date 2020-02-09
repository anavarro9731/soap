namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
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