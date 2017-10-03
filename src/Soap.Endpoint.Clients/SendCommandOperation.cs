namespace Soap.Endpoint.Clients
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Soap.Interfaces;

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
