namespace Soap.Interfaces
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface IPublishEventOperation : IBusOperation
    {
        IApiEvent Event { get; set; }
    }
}
