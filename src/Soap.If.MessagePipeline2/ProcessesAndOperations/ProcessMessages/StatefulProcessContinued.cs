namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    using System;

    public class StatefulProcessContinued : ProcessEvent, IAssociateProcessStateWithAMessage
    {
        public StatefulProcessContinued(string processType, string username, ProcessState stateBeforeHandling, Guid byMessage)
            : base(processType, username)
        {
            StateBeforeHandling = stateBeforeHandling;
            ByMessage = byMessage;
        }

        public ProcessState StateBeforeHandling { get; set; }

        public Guid ProcessStateId => StateBeforeHandling.id;

        public Guid ByMessage { get; }
    }
}