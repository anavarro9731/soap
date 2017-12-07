namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class AuthenticateUserHandler : CommandHandler<AuthenticateUser, ClientSecurityContext>
    {
        private readonly UserOperations userOperations;

        public AuthenticateUserHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task<ClientSecurityContext> Handle(AuthenticateUser message, ApiMessageMeta meta)
        {
            return await this.userOperations.AuthenticateUser(message);
        }
    }
}