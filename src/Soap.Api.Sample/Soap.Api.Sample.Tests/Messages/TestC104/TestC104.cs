//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.TestC104
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

        protected void CountDataStoreOperationsSaved(MessageLogEntry log)
        {
            log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
            log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(3); //* includes soft delete
            log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        protected void CountMessagesSaved(MessageLogEntry log)
        {
            var c100wrapped = log.UnitOfWork.BusCommandMessages.Single();
            var c100 = c100wrapped.Deserialise<C100Ping>();
            c100.PingedBy.Should().Be(nameof(P555TestUnitOfWork));

            var e150wrapped = log.UnitOfWork.BusEventMessages.Single();
            var e150 = e150wrapped.Deserialise<E150Pong>();
            e150.PongedBy.Should().Be(nameof(P555TestUnitOfWork));
        }
        
        protected async Task RecordsShouldBeReturnToOriginalState(DataStore store)
        {
            //*creations
            var lando = (await store.Read<User>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
            lando.Should().BeNull();

            var boba = (await store.Read<User>(x => x.UserName == "boba.fett")).SingleOrDefault();
            boba.Should().BeNull();

            //* updates
            var han = await store.ReadById<User>(Ids.HanSolo);
            han.FirstName.Should().Be(Aggregates.HanSolo.FirstName);
            han.LastName.Should().Be(Aggregates.HanSolo.LastName);

            var leia = await store.ReadById<User>(Ids.PrincessLeia);
            leia.Active.Should().BeTrue();

            //* deletes
            var darth = await store.ReadById<User>(Ids.DarthVader);
            darth.Should().NotBeNull();

            var luke = await store.ReadById<User>(Ids.LukeSkywalker);
            luke.Should().NotBeNull();
            luke.LastName.Should().Be("Hamill");
        }
    }
}