namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    public class AddFullyRegisteredUserHandler : CommandHandler<AddFullyRegisteredUser>
    {
        private readonly UserOperations userOperations;

        public AddFullyRegisteredUserHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task Handle(AddFullyRegisteredUser message, ApiMessageMeta meta)
        {
            var result = await this.userOperations.AddFullyRegisteredUser(
                             message.Id,
                             message.Email,
                             message.Name,
                             message.Email,
                             message.Password);
        }
    }
}