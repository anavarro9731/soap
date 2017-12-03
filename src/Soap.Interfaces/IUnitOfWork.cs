﻿namespace Soap.If.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using Soap.If.Interfaces.Messages;

    /// <summary>
    ///     this interface allows people to write a version of the UnitOfWork which
    ///     utilises a different bus technology (i.e. queues statechanges using something
    ///     other than NSB)
    /// </summary>
    public interface IUnitOfWork
    {
        Guid TransactionId { get; }

        Task ExecuteChanges();

        void PublishEvent(IApiEvent @event);

        void QueueStateChange(IQueuedStateChange queuedStateChange);

        void SendCommand(IApiCommand command);

        void SendCommandToSelf(IApiCommand command);
    }
}