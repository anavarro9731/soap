namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IBeginProcess<in TMessage, TReturnType> where TMessage : ApiCommand
    {
        Task<TReturnType> BeginProcess(TMessage message, MessageMeta meta);
    }

    public interface IBeginProcess<in TMessage> where TMessage : ApiCommand
    {
        Task BeginProcess(TMessage message, MessageMeta meta);
    }
}