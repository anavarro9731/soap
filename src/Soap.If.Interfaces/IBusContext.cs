namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    public interface IBusContext
    {
        void Publish(ApiEvent publishEvent);

        void Send(ApiCommand sendCommand);
    }
}