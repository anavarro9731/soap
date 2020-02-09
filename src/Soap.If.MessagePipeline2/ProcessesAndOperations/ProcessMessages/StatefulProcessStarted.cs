namespace Soap.If.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    public class StatefulProcessStarted : ProcessStarted
    {
        public StatefulProcessStarted(string processType, string username, ProcessState initialState)
            : base(processType, username)
        {
            InitialState = initialState;
        }

        public ProcessState InitialState { get; set; }
    }
}