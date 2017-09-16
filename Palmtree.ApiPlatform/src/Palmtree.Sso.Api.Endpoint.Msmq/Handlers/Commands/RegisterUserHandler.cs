namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes.Stateful;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;

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
