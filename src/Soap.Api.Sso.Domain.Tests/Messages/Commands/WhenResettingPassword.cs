namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Messages.ProcessMessages;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenResettingPassword
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenResettingPassword()
        {
            //arrange
            this.endPoint.AddToDatabase(TestData.User1);

            //act
            var resetPassword = new RequestPasswordReset(TestData.User1.Email);
            this.endPoint.HandleCommand(resetPassword);


            var resetPasswordFromEmail = new ResetPasswordFromEmail(TestData.User1.UserName, "new_password");
            resetPasswordFromEmail.StatefulProcessId = this.endPoint.MessageAggregator.AllMessages.OfType<StatefulProcessStarted>().Single().InitialState.id;
            this.endPoint.HandleCommand(resetPasswordFromEmail, TestData.User1);
        }

        [Fact]
        public void ItShouldSendEmail()
        {
            var emailSent = this.endPoint.MessageAggregator.CommandsSent.OfType<SendEmail>().Single();

            Assert.Equal(TestData.User1.Email, emailSent.SendTo.First());
        }

        [Fact]
        public void ItShouldStartStatefullProcess()
        {
            Assert.Contains(
                this.endPoint.MessageAggregator.AllMessages,
                e => e is StatefulProcessStarted && ((StatefulProcessStarted)e).ProcessType == "PasswordResetProcess");
        }
    }
}