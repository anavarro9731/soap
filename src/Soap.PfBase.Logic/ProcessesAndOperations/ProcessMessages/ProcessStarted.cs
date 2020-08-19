namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    public class ProcessStarted : ProcessEvent
    {
        public ProcessStarted(string processType, string username)
            : base(processType, username)
        {
            ProcessType = processType;
            Username = username;
        }
    }
}