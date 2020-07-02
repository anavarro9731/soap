namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessagePipeline;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    
    public interface IBeginProcess<in TMessage, TReturnType> where TMessage : ApiCommand
    {
        Func<TMessage, Task<TReturnType>> BeginProcess { get; }
    }

    public interface IBeginProcess<in TMessage> where TMessage : ApiCommand
    {
        Func<TMessage, Task> BeginProcess { get; }
    }
}