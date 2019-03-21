namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Newtonsoft.Json;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility;
    using Soap.If.Utility.PureFunctions;

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

            public static ClientSecurityContext Create(SecurityToken authToken, User user)
            {
                return new ClientSecurityContext
                {
                    AuthToken = SecurityToken.EncryptToken(authToken),
                    UserProfile = UserProfile.Create(user)
                };
            }

            
        }
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