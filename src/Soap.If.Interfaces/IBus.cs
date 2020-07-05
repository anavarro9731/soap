namespace Soap.Interfaces
{
    using System.Threading.Tasks;

    public interface IBus
    {
        Task CommitChanges();

        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand);

    }
}