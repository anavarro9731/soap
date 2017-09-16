namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class AuthenticateUser : ApiCommand
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
