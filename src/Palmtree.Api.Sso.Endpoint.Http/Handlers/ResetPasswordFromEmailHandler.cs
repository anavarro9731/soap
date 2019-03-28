namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
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
            return await this.passwordResetProcess.ContinueProcess<ResetPasswordFromEmail, ResetPasswordFromEmail.ClientSecurityContext>(message, meta);
        }
    }
}