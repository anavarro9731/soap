﻿namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class ResetPasswordFromEmail : ApiCommand
    {
        public ResetPasswordFromEmail(string username, string passwordResetToken, string newPassword)
        {
            PasswordResetToken = passwordResetToken;
            NewPassword = newPassword;
            Username = username;
        }

        public string NewPassword { get; set; }

        public string PasswordResetToken { get; set; }

        public string Username { get; set; }
    }

    public class SetNewPasswordValidator : AbstractValidator<ResetPasswordFromEmail>
    {
        public SetNewPasswordValidator()
        {
            RuleFor(x => x.Username).NotNull();
            RuleFor(x => x.PasswordResetToken).NotNull().NotEqual(x => x.NewPassword);
            RuleFor(x => x.NewPassword).NotNull();
        }
    }
}
