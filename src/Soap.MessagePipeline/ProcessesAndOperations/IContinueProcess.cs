﻿namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Models;

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