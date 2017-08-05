﻿namespace Palmtree.Sample.Api.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages;
    using Palmtree.ApiPlatform.ThirdPartyClients.Mailgun;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
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
            var resetToken = (string)this.endPoint.HandleCommand(resetPassword);

            var resetPasswordFromEmail = new ResetPasswordFromEmail(TestData.User1.UserName, resetToken, "new_password");
            resetPasswordFromEmail.StatefulProcessId = this.endPoint.MessageAggregator.AllMessages.OfType<StatefulProcessStarted>().Single().InitialState.id;
            this.endPoint.HandleCommand(resetPasswordFromEmail, TestData.User1);
        }

        [Fact]
        public void ItShouldSendEmail()
        {
            var emailSent = this.endPoint.MessageAggregator.CommandsSent.OfType<SendEmail>().Single();

            Assert.Equal(TestData.User1.Email, emailSent.Message.To.First().Email);
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
