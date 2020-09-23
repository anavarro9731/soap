//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.TestC104
{
    using FluentAssertions;
    using Soap.Context;
    using Soap.MessagePipeline;
    using Xunit;
    using Xunit.Abstractions;

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
            await TestMessage(Commands.TestUnitOfWork(SpecialIds.MessageDiesWhileSavingUnitOfWork), Identities.UserOne, 2);

            //assert
            Result.UnhandledError.Message.Should().Contain(SpecialIds.MessageDiesWhileSavingUnitOfWork.ToString());
        }
    }
}