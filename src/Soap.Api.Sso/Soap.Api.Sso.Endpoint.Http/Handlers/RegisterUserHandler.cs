namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Processes.Stateful;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class RegisterUserHandler : CommandHandler<RegisterUser, RegisterUser.RegistrationResult>
    {
        private readonly IStatefulProcess<UserRegistrationProcess> userRegistrationProcess;

        public RegisterUserHandler(IStatefulProcess<UserRegistrationProcess> userRegistrationProcess)
        {
            this.userRegistrationProcess = userRegistrationProcess;
        }

        protected override async Task<RegisterUser.RegistrationResult> Handle(RegisterUser message, ApiMessageMeta meta)
        {
            return await this.userRegistrationProcess.BeginProcess<RegisterUser, RegisterUser.RegistrationResult>(message, meta);
        }
    }
}