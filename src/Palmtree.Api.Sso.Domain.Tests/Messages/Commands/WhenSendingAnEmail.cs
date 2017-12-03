namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Mailer.NET.Mailer;
    using Mailer.NET.Mailer.Response;
    using Soap.DomainTests.Infrastructure;
    using Soap.Integrations.Mailgun;
    using Xunit;

    public class WhenSendingAnEmail
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly List<EmailResponse> result;

        private readonly SendEmail sendEmailCommand;

        public WhenSendingAnEmail()
        {
            //arrange            

            this.sendEmailCommand = new SendEmail(
                new Email
                {
                    From = new Contact
                    {
                        Email = "joe@schmoe.com"
                    },
                    To = new List<Contact>
                    {
                        new Contact
                        {
                            Email = "jane@schmoe.com"
                        }
                    },
                    Message = "Hi"
                });

            this.endPoint.MessageAggregator.When<EmailSender.SendingEmail>().Return(new EmailResponse());

            //act
            this.result = this.endPoint.HandleCommand(this.sendEmailCommand) as List<EmailResponse>;
        }

        [Fact]
        public void ItShouldAttemptToEmailTheUser()
        {
            var emailSending = this.endPoint.MessageAggregator.AllMessages.OfType<EmailSender.SendingEmail>().Single();

            Assert.True(emailSending.Message.To.Single().Email == this.sendEmailCommand.Message.To.Single().Email);
        }
    }
}