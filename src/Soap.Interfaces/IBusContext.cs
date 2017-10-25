namespace Soap.Interfaces
{
    using System.Threading.Tasks;

    public interface IBusContext
    {
        Task Publish(IPublishEventOperation publishEvent);

        Task Send(ISendCommandOperation sendCommand);

        Task SendLocal(ISendCommandOperation sendCommand);
    }
}