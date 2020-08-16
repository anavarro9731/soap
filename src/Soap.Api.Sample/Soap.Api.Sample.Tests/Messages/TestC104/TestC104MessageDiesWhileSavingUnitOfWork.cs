namespace Sample.Tests.Messages
{
    using System;
    using FluentAssertions;
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
            try
            {
                await ExecuteWithRetries(
                    Commands.TestUnitOfWork(SpecialIds.MessageDiesWhileSavingUnitOfWork),
                    Identities.UserOne,
                    2);

                throw new Exception("Should not reach this");
            }
            catch (Exception e)
            {
                e.Message.Should().Contain(SpecialIds.MessageDiesWhileSavingUnitOfWork.ToString());
            }
            
        }
    }
}