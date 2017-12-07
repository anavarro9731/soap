namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class ConfirmEmailHandler : CommandHandler<ConfirmEmail>
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