namespace Soap.Interfaces
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBusContext
    {
        void Publish(ApiEvent publishEvent);

        Task CommitChanges();

        void Send(ApiCommand sendCommand);
    }
}