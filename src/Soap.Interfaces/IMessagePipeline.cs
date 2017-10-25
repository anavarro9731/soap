namespace Soap.Interfaces
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IMessagePipeline
    {
        Task<object> Execute(IApiMessage message);
    }
}