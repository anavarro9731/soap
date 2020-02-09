namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IContinueProcess<in TMessage, TReturnType> where TMessage : ApiCommand
    {
        Task<TReturnType> ContinueProcess(TMessage message, MessageMeta meta);
    }

    public interface IContinueProcess<in TMessage> where TMessage : ApiCommand
    {
        Task ContinueProcess(TMessage message, MessageMeta meta);
    }
}