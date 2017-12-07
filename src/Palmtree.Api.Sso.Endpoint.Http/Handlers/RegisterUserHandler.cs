namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes.Stateful;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class RegisterUserHandler : CommandHandler<RegisterUser, RegistrationResult>
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