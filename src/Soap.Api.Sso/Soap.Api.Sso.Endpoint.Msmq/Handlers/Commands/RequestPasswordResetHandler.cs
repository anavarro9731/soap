namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sso.Domain.Logic.Processes.Stateful;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.MsmqEndpointBase;

    public class RequestPasswordResetHandler : CommandHandler<RequestPasswordReset>
    {
        private readonly IStatefulProcess<PasswordResetProcess> passwordResetProcess;

        public RequestPasswordResetHandler(IStatefulProcess<PasswordResetProcess> passwordResetProcess)
        {
            this.passwordResetProcess = passwordResetProcess;
        }

        protected override async Task Handle(RequestPasswordReset message, ApiMessageMeta meta)
        {
            new RequestPasswordResetValidator().ValidateAndThrow(message);

            await this.passwordResetProcess.BeginProcess(message, meta);
        }
    }
}