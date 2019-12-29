namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IContinueProcess<in TMessage, TReturnType> where TMessage : IApiCommand
    {
        Task<TReturnType> ContinueProcess(TMessage message, MessageMeta meta);
    }

    public interface IContinueProcess<in TMessage> where TMessage : IApiCommand
    {
        Task ContinueProcess(TMessage message, MessageMeta meta);
    }
}