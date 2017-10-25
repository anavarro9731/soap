namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Models;

    /// <summary>
    ///     a way to pull items out of the container not using a base class
    /// </summary>
    public interface IProcess<TProcess>
    {
        Task BeginProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;

        Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;
    }
}