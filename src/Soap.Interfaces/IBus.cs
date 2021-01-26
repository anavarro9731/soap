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

        Task Publish<TEvent, TContextMessage>(TEvent eventToPublish, TContextMessage contextMessage, IBusClient.EventVisibilityFlags eventVisibility = null)
            where TEvent : ApiEvent where TContextMessage : ApiMessage;

        Task Send<TCommand, TContextMessage>(
            TCommand commandToSend,
            TContextMessage contextMessage,
            bool useServiceLevelAuthority = false,
            DateTimeOffset scheduledAt = default) where TCommand : ApiCommand where TContextMessage : ApiMessage;

    }
}
