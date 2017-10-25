namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Soap.DomainTests.Infrastructure;
    using Soap.Utility;
    using Xunit;

    public class WhenDisablingAUser
    {
        private readonly User anotherUser;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenDisablingAUser()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);

            this.anotherUser = CreateAnotherUser();

            Assert.True(this.anotherUser.Active);

            this.endPoint.AddToDatabase(this.anotherUser);

            var disableUser = new DisableUser(this.anotherUser.id);

            //act
            this.endPoint.HandleCommand(disableUser, TestData.User1);
        }

        [Fact]
        public void ItShouldDisableTheUserInTheDatabase()
        {
            var single = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.id == this.anotherUser.id)).Result.Single();

            Assert.True(single.AccountIsDisabled());
        }

        private User CreateAnotherUser()
        {
            var passwordHash = SecureHmacHash.CreateFrom("secret-sauce");
            var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);
            var user = User.Create(
                "monmotha@rebelalliance.org",
                "Mon Monthma",
                passwordDetails,
                null,
                "monmothma",
                Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4"));
            user.ActiveSecurityTokens.Add(SecurityToken.Create(user.id, user.PasswordDetails.PasswordHash, DateTime.Now, new TimeSpan(0, 0, 15), true));
            return user;
        }
    }
}