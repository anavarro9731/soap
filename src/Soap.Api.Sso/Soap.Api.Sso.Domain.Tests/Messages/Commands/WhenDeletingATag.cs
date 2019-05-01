namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenDeletingATag : Test
    {
        private readonly Tag result;

        private readonly Guid TagId = Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4");

        public WhenDeletingATag()
        {
            //arrange            
            this.endPoint.AddToDatabase(Aggregates.User1);
            var Tag = new Tag
            {
                id = this.TagId, NameOfTag = "Some Tag"
            };
            this.endPoint.AddToDatabase(Tag);

            var deleteTag = new DeleteTag(this.TagId);

            //act
            this.result = this.endPoint.HandleCommand(deleteTag, Aggregates.User1);
        }

        [Fact]
        public void ItShouldReturnTheDeletedTag()
        {
            Assert.NotNull(this.result);
            Assert.Equal(this.result.id, this.TagId);
        }

        [Fact]
        public void ItShouldSoftDeleteFromTheDatabase()
        {
            var Tag = this.endPoint.QueryDatabase<Tag>(q => q.Where(x => x.id == this.TagId)).Result.Single();
            Assert.True(Tag.Active == false);
        }
    }
}