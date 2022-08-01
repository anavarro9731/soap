//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C107
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using CircuitBoard.Messages;
    using FluentAssertions;
    using Serilog;
    using Serilog.Core;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.PfBase.Messages;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC107v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC107v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
            var postCodes = new List<Enumeration>
            {
                new Enumeration("hr20dn", "HR2 0DN"),
                new Enumeration("al55ng", "AL55NG"),
                new Enumeration("ox29ju", "OX2 9JU")
            };

            var c107PostCodesMultiOptional = new EnumerationAndFlags(allEnumerations: postCodes)
            {
                AllowMultipleSelections = true
            };
            c107PostCodesMultiOptional.AddFlag(postCodes.First());
            c107PostCodesMultiOptional.AddFlag(postCodes.Last());

            TestMessage(
                new C107v1_CreateOrUpdateTestDataTypes
                {
                    C107_Boolean = false,
                    C107_Decimal = 0,
                    C107_Guid = testDataId,
                    C107_DateTime = DateTime.UtcNow,
                    C107_Long = 0,
                    C107_String = "test default",
                    C107_PostCodesSingle = new EnumerationAndFlags(postCodes.First(), postCodes),
                    C107_PostCodesSingleOptional = new EnumerationAndFlags(postCodes.Last(), postCodes),
                    C107_PostCodesMulti = new EnumerationAndFlags(postCodes.First(), postCodes),
                    C107_PostCodesMultiOptional = c107PostCodesMultiOptional,
                    C107_CustomObject = new C107v1_CreateOrUpdateTestDataTypes.Address
                    {
                        C107_Town = "pontrilas"
                    },
                    C107_File = SampleBlobs.File1,
                    C107_Image = SampleBlobs.Image1
                },
                Identities.JohnDoeAllPermissions).Wait();
        }

        [Fact]
        public async void ItShouldCreateTheRecord()
        {
            (await Result.DataStore.ReadActiveById<TestData>(testDataId)).Should().NotBeNull();
        }

        [Fact]
        public void ItShouldPublishAResponseToTheBus()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E104v1_TestDataUpserted>();
            Result.MessageBus.BusEventsPublished.Single().As<E104v1_TestDataUpserted>().E104_TestDataId.Should().Be(testDataId);
        }
        
        [Fact]
        public void ItShouldNotLogTheHouseNameField()
        {
            Result.MessageAggregator.AllMessages.OfType<ILogMessage>().Any(x => x.Text.Contains(nameof(C107v1_CreateOrUpdateTestDataTypes.Address.C107_House))).Should().BeFalse();

        }

        [Fact]
        public void ItShouldPublishAResponseToTheWebSocketClient()
        {
            Result.MessageBus.WsEventsPublished.Should().ContainSingle();
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E104v1_TestDataUpserted>();
            Result.MessageBus.WsEventsPublished.Single().As<E104v1_TestDataUpserted>().E104_TestDataId.Should().Be(testDataId);
        }
    }
}
