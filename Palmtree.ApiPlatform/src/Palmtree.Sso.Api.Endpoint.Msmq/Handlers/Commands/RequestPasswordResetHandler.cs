namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes.Stateful;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    public class RequestPasswordResetHandler : MessageHandler<RequestPasswordReset, string>
    {
        private readonly IStatefulProcess<PasswordResetProcess> passwordResetProcess;

        public RequestPasswordResetHandler(IStatefulProcess<PasswordResetProcess> passwordResetProcess)
        {
            this.passwordResetProcess = passwordResetProcess;
        }

        protected override async Task<string> Handle(RequestPasswordReset message, ApiMessageMeta meta)
        {
            return await this.passwordResetProcess.BeginProcess<RequestPasswordReset, string>(message, meta);
        }
    }
}
