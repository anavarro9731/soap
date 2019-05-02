namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Messages.ProcessMessages;
    using Soap.Integrations.MailGun;
    using Xunit;

    public class WhenResettingPassword : Test
    {
        public WhenResettingPassword()
        {
            //arrange
            this.endPoint.AddToDatabase(Aggregates.User1);

            //act
            var resetPassword = new RequestPasswordReset(Aggregates.User1.Email);
            this.endPoint.HandleCommand(resetPassword);

            var resetPasswordFromEmail = new ResetPasswordFromEmail(Aggregates.User1.UserName, "new_password");
            resetPasswordFromEmail.StatefulProcessId = this.endPoint.MessageAggregator.AllMessages.OfType<StatefulProcessStarted>().Single().InitialState.id;
            this.endPoint.HandleCommand(resetPasswordFromEmail, Aggregates.User1);
        }

        [Fact]
        public void ItShouldNotifyUser()
        {
            var emailSent = this.endPoint.MessageAggregator.CommandsSent.OfType<NotifyUsers>().Single();

            Assert.Equal(Aggregates.User1.Email, emailSent.SendTo.First());
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