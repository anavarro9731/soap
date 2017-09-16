namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes.Stateful;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

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
