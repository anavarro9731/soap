namespace Soap.Api.Sample.Tests.Messages.Commands.MessageFailedAllRetries
{
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.NotificationServer.Channels.Email;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestMessageFailedAllRetries : Test
    {
        public TestMessageFailedAllRetries(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var contextMessage = new C100v1_Ping();

            var json = contextMessage.ToJson(SerialiserIds.ApiBusMessage);

            var instanceOfMessageFailedAllRetries = new MessageFailedAllRetries
            {
                SerialiserId = SerialiserIds.ApiBusMessage.Key,
                TypeName = typeof(C100v1_Ping)
                    .ToShortAssemblyTypeName(), //* you don't want the assembly version etc since that could break deserialisation
                SerialisedMessage = json
            };

            TestMessage(
                    instanceOfMessageFailedAllRetries,
                    Identities.JohnDoeAllPermissions,
                    setupMocks: m =>
                        m.When<EmailChannel.SendingEmail>()
                         .Return(Task.CompletedTask)) //* send it with the context it would have in production code
                .Wait();
        }

        [Fact]
        public void ItShouldSendAnEmailNotification()
        {
            Result.Success.Should().BeTrue();
            var sentNotifications = Result.NotificationServer.NotificationsSent;
            sentNotifications.Should().HaveCount(1);
            sentNotifications.First().ChannelsSentTo.HasFlag(NotificationChannelTypes.Email).Should().BeTrue();
        }
    }
}
