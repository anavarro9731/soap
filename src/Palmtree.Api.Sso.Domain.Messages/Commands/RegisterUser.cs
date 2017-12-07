namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;

    public class RegisterUser : ApiCommand<RegistrationResult>
    {
        public RegisterUser(string email, string name, string password)
        {
            Email = email;
            Name = name;
            Password = password;
        }

        public string Email { get; }

        public string Name { get; }

        public string Password { get; }

        public Guid UserId { get; set; }
    }

    public class RegisterUserValidator : AbstractValidator<RegisterUser>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Name).NotEmpty().Length(5, 50);
            RuleFor(x => x.Password);
        }
    }
}