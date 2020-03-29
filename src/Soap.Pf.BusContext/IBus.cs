namespace Soap.Bus
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBus
    {
        Task CommitChanges();

        void Publish(ApiEvent publishEvent);

        void Send(ApiCommand sendCommand);

    }
}