namespace Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages
{
    public class ProcessCompleted : ProcessEvent
    {
        public ProcessCompleted(string processType, string username)
            : base(processType, username)
        {
        }
    }
}
