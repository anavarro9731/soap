namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;

    public class AuthenticateUserHandler : MessageHandler<AuthenticateUser, ClientSecurityContext>
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
