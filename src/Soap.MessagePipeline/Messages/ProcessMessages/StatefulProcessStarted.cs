namespace Soap.MessagePipeline.Messages.ProcessMessages
{
    using Soap.MessagePipeline.Models.Aggregates;

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