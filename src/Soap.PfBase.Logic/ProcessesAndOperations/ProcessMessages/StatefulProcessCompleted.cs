namespace Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages
{
    public class StatefulProcessCompleted : ProcessCompleted
    {
        public StatefulProcessCompleted(string processType, string username, ProcessState finalState)
            : base(processType, username)
        {
            FinalState = finalState;
        }

        public ProcessState FinalState { get; set; }
    }
}