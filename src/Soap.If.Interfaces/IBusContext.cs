namespace Soap.If.Interfaces
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;

    public interface IBusContext
    {
        Task Publish(ApiEvent publishEvent);

        Task Send(ApiCommand sendCommand);
    }
}