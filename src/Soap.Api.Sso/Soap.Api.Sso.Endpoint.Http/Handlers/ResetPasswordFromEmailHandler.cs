namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Processes.Stateful;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class ResetPasswordFromEmailHandler : CommandHandler<ResetPasswordFromEmail, ResetPasswordFromEmail.ClientSecurityContext>
    {
        private readonly IStatefulProcess<PasswordResetProcess> passwordResetProcess;

        public ResetPasswordFromEmailHandler(IStatefulProcess<PasswordResetProcess> passwordResetProcess)
        {
            this.passwordResetProcess = passwordResetProcess;
        }

        protected override async Task<ResetPasswordFromEmail.ClientSecurityContext> Handle(ResetPasswordFromEmail message, ApiMessageMeta meta)
        {
            var clientSecurityContext = await this.passwordResetProcess.ContinueProcess<ResetPasswordFromEmail, ClientSecurityContext>(message, meta);
            return new ResetPasswordFromEmail.ClientSecurityContext
            {
                AuthToken = clientSecurityContext.AuthToken, UserProfile = clientSecurityContext.UserProfile
            };
        }
    }
}