namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public interface IBusClient
    {
        public class EventVisibility : TypedEnumeration<EventVisibility>
        {
            public static EventVisibility WebSocketSender = Create(
                nameof(WebSocketSender),
                "Web Socket Sender (via ConversationId");

            public static EventVisibility AllWebSocketClientsNoConversationId = Create(
                nameof(AllWebSocketClientsNoConversationId),
                "All Web Socket Clients (no ConversationId");

            public static EventVisibility AllBusSubscriptions = Create(
                nameof(AllBusSubscriptions),
                "All Bus Subscriptions (could have StatefulProcessId)");
        }
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }

        
        Task Publish(ApiEvent publishEvent, EnumerationFlags eventVisibility);

        Task Send(ApiCommand sendCommand, DateTimeOffset? scheduledAt = null);
    }
}
