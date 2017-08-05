namespace Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages
{
    using Palmtree.ApiPlatform.Infrastructure.Models;

    public class StatefulProcessCompleted : ProcessCompleted
    {
        public StatefulProcessCompleted(string processType, string username)
            : base(processType, username)
        {
        }

        public ProcessState FinalState { get; set; }
    }
}
