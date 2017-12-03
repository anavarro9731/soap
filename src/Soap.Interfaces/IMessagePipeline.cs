namespace Soap.If.Interfaces
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;

    public interface IMessagePipeline
    {
        Task<object> Execute(IApiMessage message);
    }
}