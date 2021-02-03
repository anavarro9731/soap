namespace Soap.Api.Sample.Tests
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using Soap.Config;
    using Soap.Context;
    using Soap.Interfaces;

    public class TestIdentity
    {
        public TestIdentity(IdentityPermissions identityPermissions, TestProfile userProfile)
        {
            Guard.Against(identityPermissions == null || userProfile == null, $"{nameof(identityPermissions)} and {nameof(userProfile)} must both be provided");
            
            IdentityPermissions = identityPermissions;
            UserProfile = userProfile;
        }

        public string IdChainSegment => $"{AuthSchemePrefixes.Tests}://" + UserProfile.id;
        
        public IdentityPermissions IdentityPermissions { get; set; }
        
        public TestProfile UserProfile { get; set; }
        
    }
    
    public class TestProfile : Aggregate, IUserProfile
    {
        public TestProfile(Guid id, string auth0Id, string email, string firstName, string lastName)
        {
            Guard.Against(id == Guid.Empty || string.IsNullOrEmpty(auth0Id) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName), "All constructor values must be provided, no empty values");
            this.id = id;
            this.Auth0Id = auth0Id;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public TestProfile()
        {
        }

        public string Auth0Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
