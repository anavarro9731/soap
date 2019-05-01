namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenAddingATag : Test
    {
        private readonly AddATag command;

        private readonly Tag result;

        public WhenAddingATag()
        {
            //arrange            
            this.endPoint.AddToDatabase(Aggregates.User1);

            this.command = new AddATag("Some Tag")
            {
                TagId = Guid.NewGuid()
            };

            //act
            this.result = this.endPoint.HandleCommand(this.command, Aggregates.User1);
        }

        [Fact]
        public void ItShouldAddTheTagToTheDatabase()
        {
            var Tag = this.endPoint.QueryDatabase<Tag>(q => q.Where(x => x.id == this.command.TagId)).Result.SingleOrDefault();
            Assert.NotNull(Tag);
        }

        [Fact]
        public void ItShouldReturnTheNewTag()
        {
            Assert.NotNull(this.result);
            Assert.Equal(this.command.TagId, this.result.id);
        }
    }
}