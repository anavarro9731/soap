namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        IBusClient BusClient { get; } 
       
        byte MaximumNumberOfRetries { get; }
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }
        
        Task CommitChanges();

        Task Publish<T, Tm>(T eventToPublish, Tm contextMessage, EnumerationFlags eventVisibility = null)
            where T : ApiEvent where Tm : ApiMessage;

        Task Send<T>(T commandToSend) where T : ApiCommand;
    }
}
