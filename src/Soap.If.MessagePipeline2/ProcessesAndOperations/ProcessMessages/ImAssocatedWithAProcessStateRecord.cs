namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    using System;

    public interface IAssociateProcessStateWithAMessage
    {
        Guid ProcessStateId { get; }
        Guid ByMessage { get; }
    }
}