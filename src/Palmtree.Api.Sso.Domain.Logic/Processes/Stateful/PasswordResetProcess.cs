namespace Palmtree.Api.Sso.Domain.Logic.Processes.Stateful
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Mailer.NET.Mailer;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility.PureFunctions;
    using Soap.Integrations.Mailgun;

    public class PasswordResetProcess : StatefulProcess<PasswordResetProcess>,
                                        IBeginProcess<RequestPasswordReset, string>,
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

        public async Task<string> BeginProcess(RequestPasswordReset command, ApiMessageMeta meta)
        {
            {
                var tempToken = await CreateTempCredentials();

                SendEmailNotification(command.Email);

                await AddState(States.EmailSent);

                return tempToken;
            }

            async Task<string> CreateTempCredentials()
            {
                return await this.userOperations.RequestPasswordReset(command);
            }

            void SendEmailNotification(string to)
            {
                UnitOfWork.SendCommand(
                    new SendEmail(
                        new Email
                        {
                            To = new List<Contact>
                            {
                                new Contact
                                {
                                    Email = to
                                }
                            },
                            Message = $"Please click this link to reset your password: {this.config.ApiEndpointSettings.HttpEndpointUrl}/resetpassword/{ProcessId}",
                            Subject = "Password Reset Requested"
                        }));
            }
        }

        public async Task<ClientSecurityContext> ContinueProcess(ResetPasswordFromEmail command, ApiMessageMeta meta)
        {
            {
                Validate();

                var newSecurityContext = await this.userOperations.ResetPasswordFromEmail(command);

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