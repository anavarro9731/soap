namespace Soap.If.MessagePipeline
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;

    public interface IMessageHandler
    {
        Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta);
    }
}