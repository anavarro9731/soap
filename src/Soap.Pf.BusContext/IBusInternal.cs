namespace Soap.Bus
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBusInternal
    {
        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand);
    }
}