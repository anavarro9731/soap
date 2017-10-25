namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class RegisterUserHandler : MessageHandler<RegisterUser, RegistrationResult>
    {
        private readonly IStatefulProcess<UserRegistrationProcess> userRegistrationProcess;

        public RegisterUserHandler(IStatefulProcess<UserRegistrationProcess> userRegistrationProcess)
        {
            this.userRegistrationProcess = userRegistrationProcess;
        }

        protected override async Task<RegistrationResult> Handle(RegisterUser message, ApiMessageMeta meta)
        {
            return await this.userRegistrationProcess.BeginProcess<RegisterUser, RegistrationResult>(message, meta);
        }
    }
}