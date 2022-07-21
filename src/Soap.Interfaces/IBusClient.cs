namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public interface IBusClient
    {
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> BusEventsPublished { get; }
        
        List<ApiEvent> WsEventsPublished { get; }
        
        
        Task Publish(ApiEvent publishEvent, EventVisibilityFlags eventVisibility, Guid sessionId);

        Task Send(ApiCommand sendCommand, Guid sessionId, DateTimeOffset? scheduledAt = null);

        public class EventVisibility : TypedEnumeration<EventVisibility>
        {
            public static EventVisibility BroadcastToAllBusSubscriptions = Create(
                nameof(BroadcastToAllBusSubscriptions),
                "All Bus Subscriptions (could have StatefulProcessId)");

            public static EventVisibility BroadcastToAllWebSocketClientsWithNoConversationId = Create(
                nameof(BroadcastToAllWebSocketClientsWithNoConversationId),
                "All Web Socket Clients (no ConversationId");

            public static EventVisibility ReplyToWebSocketSender = Create(
                nameof(ReplyToWebSocketSender),
                "Web Socket Sender (via ConversationId");
        }

        public class EventVisibilityFlags : EnumerationFlags
        {
            public EventVisibilityFlags(EventVisibility initialState = null) : base(initialState)
            {
            } 

            public void AddFlag(EventVisibility eventVisibility)
            {
                if ((this.HasFlag(EventVisibility.BroadcastToAllWebSocketClientsWithNoConversationId)
                    && eventVisibility == EventVisibility.ReplyToWebSocketSender) ||
                    (this.HasFlag(EventVisibility.ReplyToWebSocketSender) && 
                    eventVisibility == EventVisibility.BroadcastToAllWebSocketClientsWithNoConversationId))
                {
                    throw new CircuitException(
                        "Cannot send to all web socket clients and the message sender at the same time. These options are mutually exclusive, please choose one or the other.");
                }
                EnumerationFlagsMethods.AddFlag(this, eventVisibility);
            }
        }
    }
}
