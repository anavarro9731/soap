namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

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
