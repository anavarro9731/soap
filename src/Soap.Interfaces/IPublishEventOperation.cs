namespace Soap.If.Interfaces
{
    using Soap.If.Interfaces.Messages;

    public interface IPublishEventOperation : IBusOperation
    {
        IApiEvent Event { get; set; }
    }
}