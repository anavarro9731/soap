namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IContinueProcess<in TMessage> where TMessage : ApiMessage
    {
        Func<TMessage, Task> ContinueProcess { get; }
    }
}