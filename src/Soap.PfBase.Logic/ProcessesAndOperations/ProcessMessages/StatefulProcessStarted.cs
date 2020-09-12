namespace Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages
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

        public Guid ByMessage { get; }

        public Guid ProcessStateId => StateBeforeHandling.id;

        public ProcessState StateBeforeHandling { get; set; }
    }
}