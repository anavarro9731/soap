namespace Soap.Api.Sample.Tests.Messages.Commands.C114
{
    using System;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC114v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC114v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByAddingADatabaseEntry(
                new TestData
                {
                    id = testDataId,
                    Decimal = 44.44M
                });

            TestMessage(
                    new C114v1_DeleteTestDataById
                    {
                        C114_TestDataId = testDataId
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
        }

        [Fact]
        public async void ItShouldDeleteTheEntry()
        {
            (await Result.DataStore.ReadById<TestData>(testDataId)).Active.Should().BeFalse();
        }
        
        [Fact]
        public async void ItShouldPublishAnEvent()
        {
            (await Result.DataStore.ReadById<TestData>(testDataId)).Active.Should().BeFalse();
        }
    }
}
