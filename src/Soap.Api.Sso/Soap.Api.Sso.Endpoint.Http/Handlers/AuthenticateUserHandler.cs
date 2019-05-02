namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class AuthenticateUserHandler : CommandHandler<AuthenticateUser, ResetPasswordFromEmail.ClientSecurityContext>
    {
        private readonly UserOperations userOperations;

        public AuthenticateUserHandler(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        protected override async Task<ResetPasswordFromEmail.ClientSecurityContext> Handle(AuthenticateUser message, ApiMessageMeta meta)
        {
            var securityContext = await this.userOperations.AuthenticateUser(Credentials.Create(message.Credentials.Username, message.Credentials.Password));
            return new ResetPasswordFromEmail.ClientSecurityContext
            {
                AuthToken = securityContext.AuthToken, UserProfile = securityContext.UserProfile
            };
        }
    }
}