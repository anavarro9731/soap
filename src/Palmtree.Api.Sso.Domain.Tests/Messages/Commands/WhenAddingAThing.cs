namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.DomainTests.Infrastructure;
    using Xunit;

    public class WhenAddingAThing
    {
        private readonly CreateThing command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly Thing result;

        public WhenAddingAThing()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);

            this.command = new CreateThing("Some Thing")
            {
                ThingId = Guid.NewGuid()
            };

            //act
            this.result = (Thing)this.endPoint.HandleCommand(this.command, TestData.User1);
        }

        [Fact]
        public void ItShouldAddTheThingToTheDatabase()
        {
            var thing = this.endPoint.QueryDatabase<Thing>(q => q.Where(x => x.id == this.command.ThingId)).Result.SingleOrDefault();
            Assert.NotNull(thing);
        }

        [Fact]
        public void ItShouldReturnTheNewThing()
        {
            Assert.NotNull(this.result);
            Assert.Equal(this.command.ThingId, this.result.id);
        }
    }
}