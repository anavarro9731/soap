namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPingedMultipeTimesWithTheSameMessage
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenPingedMultipeTimesWithTheSameMessage()
        {
            // Arrange
            this.command = new PingCommand("ABC");

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