﻿namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure.Models;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    public interface IBeginProcess<in TMessage, TReturnType> where TMessage : IApiCommand
    {
        Task<TReturnType> BeginProcess(TMessage message, ApiMessageMeta meta);
    }

    public interface IBeginProcess<in TMessage> where TMessage : IApiCommand
    {
        Task BeginProcess(TMessage message, ApiMessageMeta meta);
    }
}
