namespace Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages
{
    public class ProcessCompleted : ProcessEvent
    {
        public ProcessCompleted(string processType, string username)
            : base(processType, username)
        {
        }
    }
}