namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;

    public interface IAssociateProcessStateWithAMessage
    {
        Guid ByMessage { get; }

        Guid ProcessStateId { get; }
    }
}