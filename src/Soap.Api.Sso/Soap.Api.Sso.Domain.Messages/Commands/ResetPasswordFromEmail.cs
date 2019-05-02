namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Soap.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;

    public class ResetPasswordFromEmail : ApiCommand<ResetPasswordFromEmail.ClientSecurityContext>
    {
        public ResetPasswordFromEmail(string username, string newPassword)
        {
            NewPassword = newPassword;
            Username = username;
        }

        public string NewPassword { get; set; }

        public string Username { get; set; }

        public class ClientSecurityContext
        {
            public string AuthToken { get; set; }

            public UserProfile UserProfile { get; set; }
        }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<ResetPasswordFromEmail>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotNull();
                RuleFor(x => x.NewPassword).NotNull();
            }
        }
    }


}