namespace Soap.Api.Sample.Tests
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using Soap.Context;
    using Soap.Interfaces;

    public class TestIdentity
    {
        public TestIdentity(IdentityPermissions identityPermissions, TestProfile userProfile)
        {
            Guard.Against(identityPermissions == null && userProfile != null, $"{nameof(identityPermissions)} and {nameof(userProfile)} must both be provided or neither provided");
            Guard.Against(identityPermissions != null && userProfile == null, $"{nameof(identityPermissions)} and {nameof(userProfile)} must both be provided or neither provided");
            
            IdentityPermissions = identityPermissions;
            UserProfile = userProfile;
        }

        public IdentityPermissions IdentityPermissions { get; set; }
        
        public TestProfile UserProfile { get; set; }
        
    }
    
    public class TestProfile : Aggregate, IUserProfile
    {
        public TestProfile(Guid id, string auth0Id, string email, string firstName, string lastName)
        {
            this.id = id;
            this.Auth0Id = auth0Id;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            
        }

        public string Auth0Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
