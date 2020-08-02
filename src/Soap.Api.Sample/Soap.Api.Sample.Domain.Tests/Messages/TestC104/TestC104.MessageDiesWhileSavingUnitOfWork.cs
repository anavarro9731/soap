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
        /* message is retried max times and thrown out as poison when there is an error
        saving the unit of work */
        public async void MessageDiesWhileSavingUnitOfWork()
        {
            //act
            try
            {
                await ExecuteWithRetries(
                    Commands.TestUnitOfWork(SpecialIds.MessageDiesWhileSavingUnitOfWork),
                    Identities.UserOne,
                    3);

                throw new Exception("Should not reach this");
            }
            catch (Exception e)
            {
                e.Message.Should().Contain(SpecialIds.MessageDiesWhileSavingUnitOfWork.ToString());
            }

            ;
        }
    }
}