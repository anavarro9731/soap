namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessagePipeline;

    /// <summary>
    ///     a way to pull items out of the container not using a base class
    /// </summary>
    public interface IStatefulProcess<TProcess>
    {
        Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand;

        Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : ApiCommand;

        Task ContinueProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand;

        Task<TReturnType> ContinueProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : ApiCommand;
    }
}