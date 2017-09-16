namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes.Stateful;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;

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
