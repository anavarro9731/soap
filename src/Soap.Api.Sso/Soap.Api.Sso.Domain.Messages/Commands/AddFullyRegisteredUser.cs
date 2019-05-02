namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class AddFullyRegisteredUser : ApiCommand
    {
        public AddFullyRegisteredUser(Guid id, string email, string name, string password)
        {
            Id = id;
            Email = email;
            Password = password;
            Name = name;
        }

        public string Email { get; }

        public Guid Id { get; set; }

        public string Name { get; }

        public string Password { get; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<AddFullyRegisteredUser>
        {
            public Validator()
            {
                RuleFor(x => x.Email).EmailAddress();
                RuleFor(x => x.Name).NotEmpty().Length(5, 50);
                RuleFor(x => x.Password);
                RuleFor(x => x.Id).NotEmpty();
            }
        }
    }


}