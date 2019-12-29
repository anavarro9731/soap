namespace Soap.If.MessagePipeline.Messages.ProcessMessages
{
    using Soap.If.MessagePipeline.Models.Aggregates;

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