namespace Soap.Api.Sso.Domain.Tests.Messages.Ping
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenPingedMultipleTimesWithADifferentMessageButSameMessageId : Test
    {
        private readonly HttpPingCommand command;

        public WhenPingedMultipleTimesWithADifferentMessageButSameMessageId()
        {
            // Arrange
            this.command = new HttpPingCommand(1.ToString());

            var result = this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldRecordASuccessForTheFirstCommand()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.NotNull(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldThrowAnErrorForTheSecondCommand()
        {
            try
            {
                this.endPoint.HandleCommand(
                    new HttpPingCommand(2.ToString())
                    {
                        MessageId = this.command.MessageId
                    });
            }
            catch (PipelineExceptionMessages.PipelineException e)
            {
                Assert.Contains(GlobalErrorCodes.ItemIsADifferentMessageWithTheSameId.Key, e.Errors);
            }
        }
    }
}