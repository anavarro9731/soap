namespace Palmtree.Sample.Api.Domain.Logic.Processes.Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Mailer.NET.Mailer;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.ThirdPartyClients.Mailgun;
    using Palmtree.ApiPlatform.Utility.PureFunctions;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;

    public class UserRegistrationProcess : StatefulProcess<UserRegistrationProcess>, IBeginProcess<RegisterUser, RegistrationResult>, IContinueProcess<ConfirmEmail>
    {
        private readonly IApplicationConfig config;

        private readonly UserOperations userOperations;

        public UserRegistrationProcess(UserOperations userOperations, IApplicationConfig config)
        {
            this.userOperations = userOperations;
            this.config = config;
        }

        public async Task<RegistrationResult> BeginProcess(RegisterUser command, ApiMessageMeta meta)
        {
            {
                Validate();

                var newUser = await AddPartiallyRegisteredUser();

                SendEmailNotification(UserProfile.Create(newUser));

                References.UserId = newUser.id;

                return RegistrationResult.Create("User Created", UserProfile.Create(newUser), true, ProcessId);
            }

            void Validate()
            {
                new RegisterUserValidator().Validate(command);

                Guard.Against(
                    () => { return DataStore.Read<User>(q => q.Where(u => u.Email == command.Email)).Result.SingleOrDefault() != null; },
                    "User Already Exists");
            }

            async Task<User> AddPartiallyRegisteredUser()
            {
                var newUser = await this.userOperations.AddPartiallyRegisteredUser(command);

                return newUser;
            }

            void SendEmailNotification(UserProfile response)
            {
                UnitOfWork.SendCommand(
                    new SendEmail(
                        new Email
                        {
                            To = new List<Contact>
                            {
                                new Contact
                                {
                                    Email = response.Email
                                }
                            },
                            Message = $"Please click this link to reset your password: {this.config.ApiServerSettings.HttpEndpointUrl}/resetpassword/{ProcessId}",
                            Subject = "Please verify your account"
                        }));
            }
        }

        public async Task ContinueProcess(ConfirmEmail command, ApiMessageMeta meta)
        {
            await this.userOperations.ConfirmEmail(Guid.Parse(References.UserId));

            CompleteProcess(command, meta);
        }
    }
}
