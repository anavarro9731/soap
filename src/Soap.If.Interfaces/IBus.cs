namespace Soap.Interfaces
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        Task CommitChanges();

        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand);

    }
}