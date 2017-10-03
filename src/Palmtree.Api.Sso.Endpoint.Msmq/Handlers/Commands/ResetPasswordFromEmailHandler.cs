namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class ResetPasswordFromEmailHandler : MessageHandler<ResetPasswordFromEmail, ClientSecurityContext>
    {
        private readonly IStatefulProcess<PasswordResetProcess> passwordResetProcess;

        public ResetPasswordFromEmailHandler(IStatefulProcess<PasswordResetProcess> passwordResetProcess)
        {
            this.passwordResetProcess = passwordResetProcess;
        }

        protected override async Task<ClientSecurityContext> Handle(ResetPasswordFromEmail message, ApiMessageMeta meta)
        {
            return await this.passwordResetProcess.ContinueProcess<ResetPasswordFromEmail, ClientSecurityContext>(message, meta);
        }
    }
}
