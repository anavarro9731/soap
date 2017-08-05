namespace Palmtree.ApiPlatform.Interfaces
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface ISendCommandOperation : IBusOperation
    {
        IApiCommand Command { get; set; }
    }
}
