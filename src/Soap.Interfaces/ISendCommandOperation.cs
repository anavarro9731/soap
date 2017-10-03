namespace Soap.Interfaces
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface ISendCommandOperation : IBusOperation
    {
        IApiCommand Command { get; set; }
    }
}
