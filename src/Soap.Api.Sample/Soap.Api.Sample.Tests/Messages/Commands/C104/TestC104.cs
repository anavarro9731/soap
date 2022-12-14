//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Xunit.Abstractions;

    public class TestC104 : Test
    {
        public TestC104(ITestOutputHelper output)
            : base(output)
        {
            //arrange
            SetupTestByAddingADatabaseEntry(Aggregates.HanSolo);
            SetupTestByAddingADatabaseEntry(Aggregates.DarthVader);
            SetupTestByAddingADatabaseEntry(Aggregates.PrincessLeia);
            SetupTestByAddingADatabaseEntry(Aggregates.LukeSkywalker);
        }

        protected void CountDataStoreOperationsSaved(UnitOfWork uow)
        {
            uow.DataStoreCreateOperations.Count.Should().Be(2);
            uow.DataStoreUpdateOperations.Count.Should().Be(3); //* includes soft delete
            uow.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        protected void CountMessagesSaved(UnitOfWork uow)
        {
            var c100wrapped = uow.BusCommandMessages.Single();
            var c100 = c100wrapped.Deserialise<C100v1_Ping>();
            c100.C000_PingedBy.Should().Be(nameof(P201_C104__TestUnitOfWork));

            var e150wrapped = uow.BusEventMessages.Single();
            var e150 = e150wrapped.Deserialise<E100v1_Pong>();
            e150.E000_PongedBy.Should().Be(nameof(P201_C104__TestUnitOfWork));
        }
        
        protected async Task RecordsShouldBeReturnToOriginalState(DataStore store)
        {
            //*creations
            var lando = (await store.Read<UserProfile>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
            lando.Should().BeNull();

            var boba = (await store.Read<UserProfile>(x => x.UserName == "boba.fett")).SingleOrDefault();
            boba.Should().BeNull();

            //* updates
            var han = await store.ReadById<UserProfile>(Ids.HanSolo);
            han.FirstName.Should().Be(Aggregates.HanSolo.FirstName);
            han.LastName.Should().Be(Aggregates.HanSolo.LastName);

            var leia = await store.ReadById<UserProfile>(Ids.PrincessLeia);
            leia.Active.Should().BeTrue();

            //* deletes
            var darth = await store.ReadById<UserProfile>(Ids.DarthVader);
            darth.Should().NotBeNull();

            var luke = await store.ReadById<UserProfile>(Ids.LukeSkywalker);
            luke.Should().NotBeNull();
            luke.LastName.Should().Be("Hamill");
        }
    }
}
