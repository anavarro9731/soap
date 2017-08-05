namespace Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages
{
    using Palmtree.ApiPlatform.Infrastructure.Models;

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
