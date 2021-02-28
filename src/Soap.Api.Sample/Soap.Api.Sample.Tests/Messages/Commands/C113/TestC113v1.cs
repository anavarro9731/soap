namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC113v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC113v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByAddingADatabaseEntry(new TestData()
            {
                id = testDataId,
                Decimal = 44.44M
            });
            
            TestMessage(
                new C113v1_GetC107FormDataForEdit()
                {
                    C113_TestDataId = testDataId
                },
                Identities.JohnDoeAllPermissions,
                setupMocks: messageAggregatorForTesting =>
                    {
                    messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                    
                    }).Wait();
        }

        [Fact]
        public void ItShouldNotPublishAResponseToTheBus()
        {
            Result.MessageBus.BusEventsPublished.Should().BeEmpty();
        }

        [Fact]
        public void ItShouldPublishAResponseToTheWebSocketClient()
        {
            Result.MessageBus.WsEventsPublished.Should().ContainSingle();
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E103v1_GotC107FormData>();
            Result.MessageBus.WsEventsPublished.Single()
                  .DirectCast<E103v1_GotC107FormData>()
                  .E000_FieldData.Single(x => x.Name == nameof(C107v1_CreateOrUpdateTestDataTypes.C107_Decimal).ToCamelCase())
                  .InitialValue.Should()
                  .Be(44.44M);  
            (Result.MessageBus.WsEventsPublished.Single() as E103v1_GotC107FormData).E000_CommandName.Should()
                                                                                .Be(
                                                                                    typeof(C107v1_CreateOrUpdateTestDataTypes)
                                                                                        .ToShortAssemblyTypeName());
        }
    }
}
