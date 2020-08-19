﻿namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    /// can only be started with a command but can be continued by anything
    public interface IBeginProcess<in TMessage> where TMessage : ApiCommand
    {
        Func<TMessage, Task> BeginProcess { get; }
    }
}