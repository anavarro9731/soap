namespace Soap.If.Interfaces
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;

    public interface IBusContext
    {
        Task Publish(IApiEvent publishEvent);

        Task Send(IApiCommand sendCommand);

        Task SendLocal(IApiCommand sendCommand);

        bool IsOneWay { get; set; }
    }
}