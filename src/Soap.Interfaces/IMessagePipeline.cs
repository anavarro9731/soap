namespace Soap.Interfaces
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface IMessagePipeline
    {
        Task<object> Execute(IApiMessage message);
    }
}
