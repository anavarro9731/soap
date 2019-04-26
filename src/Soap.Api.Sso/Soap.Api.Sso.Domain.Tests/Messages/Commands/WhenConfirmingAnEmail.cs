namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenConfirmingAnEmail
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly RegisterUser.RegistrationResult result;

        public WhenConfirmingAnEmail()
        {
            //arrange            
            var email = "joe@schmoe.com";
            var registerUserCommand = new RegisterUser(email, "Joe Schmoe", "password");
            this.result = this.endPoint.HandleCommand(registerUserCommand) as RegisterUser.RegistrationResult;

            var confirmEmail = new ConfirmEmail(this.result.ProcessId);
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(u => u.id == this.result.User.id)).Result.Single();

            //act
            this.endPoint.HandleCommand(confirmEmail);
        }

        [Fact]
        public void ItShouldConfirmTheUser()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(u => u.id == this.result.User.id)).Result.Single();
            Assert.True(user.Status.HasState(User.UserStates.EmailConfirmed));
        }
    }
}