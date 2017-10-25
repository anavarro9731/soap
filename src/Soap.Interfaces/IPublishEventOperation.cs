namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    public interface IPublishEventOperation : IBusOperation
    {
        IApiEvent Event { get; set; }
    }
}