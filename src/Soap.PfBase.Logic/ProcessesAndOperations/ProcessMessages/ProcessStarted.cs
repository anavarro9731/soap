namespace Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages
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