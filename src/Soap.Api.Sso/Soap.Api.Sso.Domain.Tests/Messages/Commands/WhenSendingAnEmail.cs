namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Mailer.NET.Mailer.Response;
    using Soap.Integrations.MailGun;
    using Xunit;

    public class WhenSendingAnEmail : Test
    {
        private readonly SendEmail sendEmailCommand;

        public WhenSendingAnEmail()
        {
            //arrange            

            this.sendEmailCommand = new SendEmail("Hi", "Hi", "jane@schmoe.com");

            this.endPoint.MessageAggregator.When<EmailSender.SendingEmail>().Return(new EmailResponse());

            //act
            this.endPoint.HandleCommand(this.sendEmailCommand);
        }

        [Fact]
        public void ItShouldAttemptToEmailTheUser()
        {
            var emailSending = this.endPoint.MessageAggregator.AllMessages.OfType<EmailSender.SendingEmail>().Single();

            Assert.True(emailSending.SendTo.Single() == this.sendEmailCommand.SendTo.Single());
        }
    }
}