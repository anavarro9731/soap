namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.DomainTests.Infrastructure;
    using Xunit;

    public class WhenDeletingAThing
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly Thing result;

        private readonly Guid thingId = Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4");

        public WhenDeletingAThing()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);
            var thing = new Thing
            {
                id = this.thingId,
                NameOfThing = "Some Thing"
            };
            this.endPoint.AddToDatabase(thing);

            var deleteThing = new DeleteThing(this.thingId);

            //act
            this.result = (Thing)this.endPoint.HandleCommand(deleteThing, TestData.User1);
        }

        [Fact]
        public void ItShouldReturnTheDeletedThing()
        {
            Assert.NotNull(this.result);
            Assert.Equal(this.result.id, this.thingId);
        }

        [Fact]
        public void ItShouldSoftDeleteFromTheDatabase()
        {
            var thing = this.endPoint.QueryDatabase<Thing>(q => q.Where(x => x.id == this.thingId)).Result.Single();
            Assert.True(thing.Active == false);
        }
    }
}
