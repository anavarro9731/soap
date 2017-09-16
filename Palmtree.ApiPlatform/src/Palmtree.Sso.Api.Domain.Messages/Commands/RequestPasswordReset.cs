namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

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
