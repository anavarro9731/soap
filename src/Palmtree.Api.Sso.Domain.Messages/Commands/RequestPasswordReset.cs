namespace Palmtree.Api.Sso.Domain.Messages.Commands
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
    }

    public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordReset>
    {
        public RequestPasswordResetValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}