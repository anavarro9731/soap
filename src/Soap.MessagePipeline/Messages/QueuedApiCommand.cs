﻿namespace Soap.MessagePipeline.Messages
{
    using System;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class QueuedApiCommand : QueuedStateChange
    {
        public IApiCommand Command { get; set; } //really just here for reference in debugging as all detail is in the closure
    }

    public class SendCommandOperation : ISendCommandOperation
    {
        public SendCommandOperation(IApiCommand command)
        {
            Command = command;
        }

        public IApiCommand Command { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }
    }
}