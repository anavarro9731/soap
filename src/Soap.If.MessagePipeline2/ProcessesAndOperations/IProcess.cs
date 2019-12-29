namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;

    /// <summary>
    ///     a way to pull items out of the container not using a base class
    /// </summary>
    public interface IProcess<TProcess>
    {
        Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : IApiCommand;

        Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : IApiCommand;
    }
}