namespace Sample.Tests.Messages.Queries
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Messages.Events;
    using Sample.Models.Aggregates;
    using Sample.Models.Constants;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class GetMessageLogItem : Test
    {
        public GetMessageLogItem(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Queries.GetMessageLogItem, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSendAGotMessageEvent()
        {
            this.Result.MessageBus.Events.Should().ContainSingle();
            this.Result.MessageBus.Events.Single().Should().BeOfType<GotMessageLogItemEvent>();
        }
    }
}