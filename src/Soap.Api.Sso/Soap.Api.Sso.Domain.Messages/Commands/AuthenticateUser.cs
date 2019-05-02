namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System.Runtime.InteropServices;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class AuthenticateUser : ApiCommand<ResetPasswordFromEmail.ClientSecurityContext>
    {
        public AuthenticateUser(UserCredentials credentials)
        {
            Credentials = credentials;
        }

        public UserCredentials Credentials { get; set; }

        public class UserCredentials
        {
            private UserCredentials()
            {
            }

            public string Password { get; set; }

            public string Username { get; set; }

            public static UserCredentials Create(string username, string password)
            {
                return new UserCredentials
                {
                    Username = username,
                    Password = password
                };
            }
        }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<AuthenticateUser>
        {
            public Validator()
            {
                RuleFor(x => x.Credentials).NotNull();
                RuleFor(x => x.Credentials.Password).NotEmpty();
                RuleFor(x => x.Credentials.Username).NotEmpty();
            }
        }
    }

}