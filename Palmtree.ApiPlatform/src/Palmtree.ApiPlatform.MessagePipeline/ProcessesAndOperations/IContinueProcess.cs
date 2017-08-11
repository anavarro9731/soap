namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure.Models;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IContinueProcess<in TMessage, TReturnType> where TMessage : IApiCommand
    {
        Task<TReturnType> ContinueProcess(TMessage message, ApiMessageMeta meta);
    }

    public interface IContinueProcess<in TMessage> where TMessage : IApiCommand
    {
        Task ContinueProcess(TMessage message, ApiMessageMeta meta);
    }
}
