namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    using System;

    public class StatefulProcessStarted : ProcessStarted, IAssociateProcessStateWithAMessage
    {
        public StatefulProcessStarted(string processType, string username, ProcessState stateBeforeHandling, Guid byMessage)
            : base(processType, username)
        {
            StateBeforeHandling = stateBeforeHandling;
            ByMessage = byMessage;
        }

        public ProcessState StateBeforeHandling { get; set; }

        public Guid ByMessage { get; }

        public Guid ProcessStateId => StateBeforeHandling.id;
    }
}