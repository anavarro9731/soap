namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        List<ApiCommand> CommandsSent { get; }

        List<ApiEvent> EventsPublished { get; }

        Task CommitChanges();

        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand);
    }
}