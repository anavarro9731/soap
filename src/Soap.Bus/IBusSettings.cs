namespace Soap.Bus
{
    using CircuitBoard.MessageAggregator;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Soap.Interfaces;

    public interface IBusSettings
    {
        byte NumberOfApiMessageRetries { get; set; }

        IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage, IAsyncCollector<SignalRMessage> signalRBinding);
    }
}
