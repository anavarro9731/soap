namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        byte MaximumNumberOfRetries { get; }
        
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }

        Task CommitChanges();

        Task Publish<T>(T publishEvent) where T: ApiEvent;

        Task Send<T>(T sendCommand) where T: ApiCommand;
    }
}