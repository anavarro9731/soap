namespace Palmtree.Sample.Api.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Messages.Generic;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Xunit;

    public class WhenAddingAThing
    {
        private readonly CreateAggregate<Thing> command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly Thing result;

        public WhenAddingAThing()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);

            var newThing = new Thing
            {
                id = Guid.NewGuid(),
                NameOfThing = "Some Thing"
            };

            this.command = CreateAggregate<Thing>.Create(newThing);

            //act
            this.result = (Thing)this.endPoint.HandleCommand(this.command, TestData.User1);
        }

        [Fact]
        public void ItShouldAddTheThingToTheDatabase()
        {
            var thing = this.endPoint.QueryDatabase<Thing>(q => q.Where(x => x.id == this.command.Model.id)).Result.SingleOrDefault();
            Assert.NotNull(thing);
        }

        [Fact]
        public void ItShouldReturnTheNewThing()
        {
            Assert.NotNull(this.result);
            Assert.Equal(this.command.Model.id, this.result.id);
        }
    }
}
