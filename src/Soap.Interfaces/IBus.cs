namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Bus;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        IBusClient BusClient { get; } 
       
        byte MaximumNumberOfRetries { get; }
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }
        
        Task CommitChanges();

        Task Publish<T>(T eventToPublish) where T : ApiEvent;

        Task Send<T>(T commandToSend) where T : ApiCommand;
    }
}