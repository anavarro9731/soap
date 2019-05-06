﻿namespace Soap.Api.Sso.Domain.Logic.Processes.Stateful
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility.PureFunctions;
    using Soap.Integrations.MailGun;

    public class UserRegistrationProcess : StatefulProcess<UserRegistrationProcess>,
                                           IBeginProcess<RegisterUser, RegisterUser.RegistrationResult>,
                                           IContinueProcess<ConfirmEmail>
    {
        private readonly IApplicationConfig config;

        private readonly UserOperations userOperations;

        public UserRegistrationProcess(UserOperations userOperations, IApplicationConfig config)
        {
            this.userOperations = userOperations;
            this.config = config;
        }

        public async Task<RegisterUser.RegistrationResult> BeginProcess(RegisterUser command, ApiMessageMeta meta)
        {
            {
                Validate();

                var newUser = await AddPartiallyRegisteredUser();

                SendNotification(UserProfile.Create(newUser));

                References.UserId = newUser.id;

                return RegisterUser.RegistrationResult.Create("User Created", UserProfile.Create(newUser), true, ProcessId);
            }

            void Validate()
            {
                Guard.Against(() => { return DataStoreReadOnly.Read<User>(u => u.Email == command.Email).Result.SingleOrDefault() != null; }, "User Already Exists");
            }

            async Task<User> AddPartiallyRegisteredUser()
            {
                var newUser = await this.userOperations.AddPartiallyRegisteredUser(command.Email, command.Name, command.Password);

                return newUser;
            }

            void SendNotification(UserProfile response)
            {
                var message = $"Please click this link to reset your password: {this.config.ApiEndpointSettings.HttpEndpointUrl}/resetpassword/{ProcessId}";
                var subject = "Please verify your account";

                UnitOfWork.SendCommand(new NotifyUsers(message, subject, response.Email));
            }
        }

        public async Task ContinueProcess(ConfirmEmail command, ApiMessageMeta meta)
        {
            await this.userOperations.ConfirmEmail(Guid.Parse(References.UserId));

            CompleteProcess(command, meta);
        }
    }
}