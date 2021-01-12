namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        IBusClient BusClient { get; } 
       
        byte MaximumNumberOfRetries { get; }
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> BusEventsPublished { get; }
        
        List<ApiEvent> WsEventsPublished { get; }
        
        Task CommitChanges();

        Task Publish<T, Tm>(T eventToPublish, Tm contextMessage, IBusClient.EventVisibilityFlags eventVisibility = null)
            where T : ApiEvent where Tm : ApiMessage;

        Task Send<T>(T commandToSend, DateTimeOffset scheduledAt = default) where T : ApiCommand;
    }
}
