namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    using System;

    public interface IAssociateProcessStateWithAMessage
    {
        Guid ByMessage { get; }

        Guid ProcessStateId { get; }
    }
}