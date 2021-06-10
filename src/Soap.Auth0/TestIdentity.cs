namespace Soap.Auth0
{
    using System;
    using System.Collections.Concurrent;
    using DataStore.Interfaces.LowLevel;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Operations;

    public class TestIdentity
    {
        /* this is to compensate for the fact that Aesops.Encrypt will for the same ciphertext and key generate a different result each time due to random salt
        which is good, and rather than weaken that by offering an overload to fix the salt which is used with production code, i decided to tinker with the testidentity instead.
        I would have cached the entire testentity but it cant be immutable because the profile is saved to the db, and if its not immutable and one test changes
        the cache other tests will break if its static. a cache with a scope of each test run would work but then you would need to hide the global Identities. class
        so you dont use that copy by mistake which is just all too hard, this is i think the safest most encapsulated scenario i could think of in s */
        private static readonly ConcurrentDictionary<string, string> cache_idTokens = new ConcurrentDictionary<string, string>();

        private static readonly ConcurrentDictionary<string, string> cache_accessTokens = new ConcurrentDictionary<string, string>();
        
        public TestIdentity(IdentityPermissions identityPermissions, TestProfile userProfile)
        {
            Guard.Against(
                identityPermissions == null || userProfile == null,
                $"{nameof(identityPermissions)} and {nameof(userProfile)} must both be provided");

            IdentityPermissions = identityPermissions;
            UserProfile = userProfile;
        }

        public string AccessToken
        {
            get
            {
                var profileId = UserProfile.id.ToString();
                return cache_accessTokens.GetOrAdd(profileId, RandomOps.RandomString(64));
            }
        }

        public string IdChainSegment => $"{AuthSchemePrefixes.Tests}://" + UserProfile.id;

        public IdentityPermissions IdentityPermissions { get; }

        public TestProfile UserProfile { get; }

        public string IdToken(string encryptionKey)
        {
            var profileId = UserProfile.id.ToString();
            return cache_idTokens.GetOrAdd(profileId, AesOps.Encrypt(profileId, encryptionKey));
        }
    }

    public class TestProfile : Aggregate, IUserProfile
    {
        public TestProfile(Guid id, string auth0Id, string email, string firstName, string lastName)
        {
            this.id = id;
            Auth0Id = auth0Id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
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
