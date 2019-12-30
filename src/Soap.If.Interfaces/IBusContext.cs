namespace Soap.If.Interfaces
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;

    public interface IBusContext
    {
        void Publish(ApiEvent publishEvent);

        void Send(ApiCommand sendCommand);
    }
}