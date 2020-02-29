namespace Soap.Interfaces.Messages
{
    using System.Threading.Tasks;

    public interface IMessageFunctions
    {
        IMapErrorCodesFromDomainToMessageErrorCodes GetErrorCodeMapper();

        Task HandleFinalFailure(ApiMessage msg);

        Task Handle(ApiMessage msg);

        void Validate(ApiMessage msg);
    }
}