namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPingedMultipleTimesWithADifferentMessageButSameMessageId
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenPingedMultipleTimesWithADifferentMessageButSameMessageId()
        {
            // Arrange
            this.command = new PingCommand("ABC");

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
                    new PingCommand("DEF")
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