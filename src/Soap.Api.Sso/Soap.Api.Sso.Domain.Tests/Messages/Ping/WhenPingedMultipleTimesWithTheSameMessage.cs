namespace Soap.Api.Sso.Domain.Tests.Messages.Ping
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenPingedMultipleTimesWithTheSameMessage : Test
    {
        private readonly HttpPingCommand command;

        public WhenPingedMultipleTimesWithTheSameMessage()
        {
            // Arrange
            this.command = new HttpPingCommand(nameof(WhenPingedMultipleTimesWithTheSameMessage));

            this.endPoint.HandleCommand(this.command);
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
                this.endPoint.HandleCommand(this.command);
            }
            catch (PipelineExceptionMessages.PipelineException e)
            {
                Assert.Contains(GlobalErrorCodes.MessageHasAlreadyBeenProcessedSuccessfully.Key, e.Errors);
            }
        }
    }
}