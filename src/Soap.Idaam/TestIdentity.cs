namespace Soap.Idaam
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Operations;

    public class TestIdentity
    {
        /* Using concurrent static dictionaries for the tokens associated to each identity ensure you receive the same id and access tokens
         for the same user, no matter how many instances you create. This is to compensate for the fact that Aesops.Encrypt will for the 
         same ciphertext and key generate a different result each time due to the random salt which is good, so rather than having to decrypt
         all the tokens to compare values,I did this instead. arguably IRL the tokens will change over time anyway, but when you are running tests the
         window of time is seconds. */

        private static readonly ConcurrentDictionary<string, string> cache_accessTokens = new ConcurrentDictionary<string, string>();
        
        private static readonly ConcurrentDictionary<string, string> cache_idTokens = new ConcurrentDictionary<string, string>();

        public TestIdentity(List<RoleInstance> roleInstances, IUserProfile userProfile)
        {
            Guard.Against(roleInstances == null || userProfile == null, $"{nameof(roleInstances)} and {nameof(userProfile)} must both be provided");

            RoleInstances = roleInstances;
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

        public string IdChainSegment => $"{AuthSchemePrefixes.User}://" + UserProfile.Email;

        public List<RoleInstance> RoleInstances { get; }

        public IUserProfile UserProfile { get; }

        public string IdToken(string encryptionKey)
        {
            var profileId = UserProfile.id.ToString();
            return cache_idTokens.GetOrAdd(profileId, AesOps.Encrypt(profileId, encryptionKey));
        }
    }
}