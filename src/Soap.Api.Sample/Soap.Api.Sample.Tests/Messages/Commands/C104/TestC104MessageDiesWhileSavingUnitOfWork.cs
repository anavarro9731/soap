//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC104MessageDiesWhileSavingUnitOfWork : TestC104
    {
        public TestC104MessageDiesWhileSavingUnitOfWork(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message fails due to guard 
           Run 2 same
           Run 3 same  */
        public async void MessageDiesWhileSavingUnitOfWork()
        {
            //act
            await TestMessage(Commands.TestUnitOfWork(SpecialIds.MessageDiesWhileSavingUnitOfWork), Identities.JohnDoeAllPermissions, 2);

            //assert
            Result.Success.Should().BeFalse();
            Result.MessageBus.CommandsSent.Single().Should().BeOfType<MessageFailedAllRetries>();
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E001v1_MessageFailed>();
            Result.UnhandledError.Message.Should().Contain(SpecialIds.MessageDiesWhileSavingUnitOfWork.ToString());
            
        }
    }
}
