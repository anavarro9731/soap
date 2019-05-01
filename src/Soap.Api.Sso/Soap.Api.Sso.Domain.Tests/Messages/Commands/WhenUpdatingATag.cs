namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenUpdatingATag : Test
    {
        private readonly Guid TagId = Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4");

        public WhenUpdatingATag()
        {
            //arrange            
            this.endPoint.AddToDatabase(Aggregates.User1);

            var Tag = new Tag
            {
                id = this.TagId, NameOfTag = "Sme Tag"
            };

            this.endPoint.AddToDatabase(Tag);

            var changeTag = new UpdateNameOfTag(this.TagId, "Some Tag");

            //act
            this.endPoint.HandleCommand(changeTag, Aggregates.User1);
        }

        [Fact]
        public void ItShouldUpdateTheTagInTheDatabase()
        {
            var single = this.endPoint.QueryDatabase<Tag>(q => q.Where(x => x.id == this.TagId)).Result.Single();

            Assert.True(single.NameOfTag == "Some Tag");
        }
    }
}