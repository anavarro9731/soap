namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Mailer.NET.Mailer;
    using Mailer.NET.Mailer.Response;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenSendingAnEmail
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly SendEmail sendEmailCommand;

        public WhenSendingAnEmail()
        {
            //arrange            

            this.sendEmailCommand = new SendEmail(
                "Hi",
                "Hi", "jane@schmoe.com");

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