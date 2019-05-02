namespace Soap.Api.Sso.Domain.Logic.Processes.Stateful
{
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility.PureFunctions;
    using Soap.Integrations.MailGun;

    public class PasswordResetProcess : StatefulProcess<PasswordResetProcess>,
                                        IBeginProcess<RequestPasswordReset>,
                                        IContinueProcess<ResetPasswordFromEmail, ClientSecurityContext>

    {
        private readonly IApplicationConfig config;

        private readonly UserOperations userOperations;

        public PasswordResetProcess(UserOperations userOperations, IApplicationConfig config)
        {
            this.userOperations = userOperations;
            this.config = config;
        }

        private enum States
        {
            EmailSent = 0,

            PasswordReset = 1
        }

        public async Task BeginProcess(RequestPasswordReset command, ApiMessageMeta meta)
        {
            {
                await SetUserState();

                NotifyUser(command.Email);

                await AddState(States.EmailSent);
            }

            async Task SetUserState()
            {
                await this.userOperations.RequestPasswordReset(command.Email, ProcessId);
            }

            void NotifyUser(string to)
            {
                var message = $"Please click this link to reset your password: {this.config.ApiEndpointSettings.HttpEndpointUrl}/resetpassword/{ProcessId}";
                var subject = "Password Reset Requested";

                UnitOfWork.SendCommand(new NotifyUsers(message, subject, to));
            }
        }

        public async Task<ClientSecurityContext> ContinueProcess(ResetPasswordFromEmail command, ApiMessageMeta meta)
        {
            {
                Validate();

                var newSecurityContext = await this.userOperations.ResetPasswordFromEmail(command.Username, command.NewPassword, command.StatefulProcessId.Value);

                await AddState(States.PasswordReset);

                CompleteProcess(command, meta);

                return newSecurityContext;
            }

            void Validate()
            {
                Guard.Against(!HasState(States.EmailSent), "Cannot Set Password When User Has Not Requested It");
            }
        }
    }
}