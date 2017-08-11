namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure.Models;

    /// <summary>
    ///     a way to pull items out of the container not using a base class
    /// </summary>
    public interface IStatefulProcess<TProcess>
    {
        Task BeginProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;

        Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;

        Task ContinueProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;

        Task<TReturnType> ContinueProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand;
    }
}
