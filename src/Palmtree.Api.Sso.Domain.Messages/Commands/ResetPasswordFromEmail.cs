namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;

    public class ResetPasswordFromEmail : ApiCommand<ClientSecurityContext>
    {
        public ResetPasswordFromEmail(string username, string newPassword)
        {
            NewPassword = newPassword;
            Username = username;
        }

        public string NewPassword { get; set; }
        public string Username { get; set; }
    }

    public class ResetPasswordFromEmailValidator : AbstractValidator<ResetPasswordFromEmail>
    {
        public ResetPasswordFromEmailValidator()
        {
            RuleFor(x => x.Username).NotNull();
            RuleFor(x => x.NewPassword).NotNull();
        }
    }
}