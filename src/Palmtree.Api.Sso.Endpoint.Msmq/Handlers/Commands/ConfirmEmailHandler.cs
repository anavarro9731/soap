namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class ConfirmEmailHandler : MessageHandler<ConfirmEmail>
    {
        private readonly IStatefulProcess<UserRegistrationProcess> userRegistrationProcess;

        public ConfirmEmailHandler(IStatefulProcess<UserRegistrationProcess> userRegistrationProcess)
        {
            this.userRegistrationProcess = userRegistrationProcess;
        }

        protected override async Task Handle(ConfirmEmail message, ApiMessageMeta meta)
        {
            await this.userRegistrationProcess.ContinueProcess(message, meta);
        }
    }
}