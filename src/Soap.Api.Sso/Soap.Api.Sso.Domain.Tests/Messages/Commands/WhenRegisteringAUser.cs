namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Integrations.MailGun;
    using Xunit;

    public class WhenRegisteringAUser : Test
    {
        private readonly string email;

        private readonly RegisterUser registerUserCommand;

        public WhenRegisteringAUser()
        {
            //arrange            
            this.email = "joe@schmoe.com";
            this.registerUserCommand = new RegisterUser(this.email, "Joe Schmoe", "password");

            //act
            this.endPoint.HandleCommand(this.registerUserCommand);
        }

        [Fact]
        public void ItShouldAddTheUserInTheDatabase()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.UserName == this.registerUserCommand.Email)).Result.SingleOrDefault();

            Assert.NotNull(user);
        }

        [Fact]
        public void ItShouldSendAnEmailToTheNewUser()
        {
            var emailSent = this.endPoint.MessageAggregator.CommandsSent.OfType<NotifyUsers>().Single();

            Assert.Equal(this.email, emailSent.SendTo.First());
        }
    }
}