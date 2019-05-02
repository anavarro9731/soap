namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class RequestPasswordReset : ApiCommand
    {
        public RequestPasswordReset(string email)
        {
            Email = email;
        }

        public string Email { get; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }
    }

    public class Validator : AbstractValidator<RequestPasswordReset>
    {
        public Validator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}