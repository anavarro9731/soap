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

        public Guid ByMessage { get; }

        public Guid ProcessStateId => StateBeforeHandling.id;

        public ProcessState StateBeforeHandling { get; set; }
    }
}