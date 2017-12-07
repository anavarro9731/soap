namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;

    public class AuthenticateUser : ApiCommand<ClientSecurityContext>
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
    }

    public class AuthenticateUserValidator : AbstractValidator<AuthenticateUser>
    {
        public AuthenticateUserValidator()
        {
            RuleFor(x => x.Credentials).NotNull();
            RuleFor(x => x.Credentials.Password).NotEmpty();
            RuleFor(x => x.Credentials.Username).NotEmpty();
        }
    }
}