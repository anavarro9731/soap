namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenUpdatingAThing
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly Guid thingId = Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4");

        public WhenUpdatingAThing()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);

            var thing = new Thing
            {
                Id = this.thingId,
                NameOfThing = "Sme Thing"
            };

            this.endPoint.AddToDatabase(thing);

            var changeThing = new UpdateNameOfThing(this.thingId, "Some Thing");

            //act
            this.endPoint.HandleCommand(changeThing, TestData.User1);
        }

        [Fact]
        public void ItShouldUpdateTheThingInTheDatabase()
        {
            var single = this.endPoint.QueryDatabase<Thing>(q => q.Where(x => x.Id == this.thingId)).Result.Single();

            Assert.True(single.NameOfThing == "Some Thing");
        }
    }
}