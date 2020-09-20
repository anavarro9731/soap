namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public interface IBusClient
    {
        
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }

        
        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand, DateTimeOffset? scheduledAt = null);
    }
}