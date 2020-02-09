namespace Soap.If.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    public class ProcessCompleted : ProcessEvent
    {
        public ProcessCompleted(string processType, string username)
            : base(processType, username)
        {
        }
    }
}