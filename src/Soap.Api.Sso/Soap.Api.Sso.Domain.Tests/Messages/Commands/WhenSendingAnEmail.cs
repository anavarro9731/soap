namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Mailer.NET.Mailer.Response;
    using Soap.Integrations.MailGun;
    using Xunit;

    public class WhenSendingAnEmail : Test
    {
        private readonly NotifyUsers notifyUsersCommand;

        public WhenSendingAnEmail()
        {
            //arrange            

            this.notifyUsersCommand = new NotifyUsers("Hi", "Hi", "jane@schmoe.com");

            this.endPoint.MessageAggregator.When<EmailSender.SendingEmail>().Return(new EmailResponse());

            //act
            this.endPoint.HandleCommand(this.notifyUsersCommand);
        }

        [Fact]
        public void ItShouldAttemptToEmailTheUser()
        {
            var emailSending = this.endPoint.MessageAggregator.AllMessages.OfType<EmailSender.SendingEmail>().Single();

            Assert.True(emailSending.SendTo.Single() == this.notifyUsersCommand.SendTo.Single());
        }
    }
}