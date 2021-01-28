namespace Soap.Api.Sample.Tests
{
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;

    public class TestIdentity
    {
        public TestIdentity(ApiIdentity apiIdentity, TestProfile userProfile)
        {
            ApiIdentity = apiIdentity;
            UserProfile = userProfile;
        }

        public ApiIdentity ApiIdentity { get; set; }
        
        public TestProfile UserProfile { get; set; }
    }
    
    public class TestProfile : Aggregate, IUserProfile
    {
        public TestProfile(string auth0Id, string email, string firstName, string lastName)
        {
            Auth0Id = auth0Id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }

        public string Auth0Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
