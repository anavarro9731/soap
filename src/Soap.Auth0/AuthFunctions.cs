namespace Soap.Context
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Soap.Api.Sample.Tests;
    using Soap.Auth0;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class AuthFunctions
    {
        public static TestIdentity TestIdentity; //* set by test to use to lookup identities
        
        private static ServiceLevelAuthority cache;

        public static async Task<(IdentityPermissions IdentityPermissions, TUserProfile UserProfile)>
            AuthenticateandAuthoriseOrThrow<TUserProfile>(
                ApiMessage message,
                ApplicationConfig applicationConfig,
                DataStore dataStore,
                ISecurityInfo securityInfo) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            IdentityPermissions identityPermissions = null;
            TUserProfile userProfile = null;

            var shouldAuthorise = IsSubjectToAuthorisation(message, applicationConfig);

            if (shouldAuthorise)
            {
                Guard.Against(
                    shouldAuthorise && (message.Headers.GetIdentityChain() == null || message.Headers.GetIdentityToken() == null
                                                                                   || message.Headers.GetAccessToken() == null),
                    "All Authorisation headers not provided but message is not exempt from authorisation");

                Guard.Against(
                    Regex.IsMatch(
                        message.Headers.GetIdentityChain(),
                        $"^({AuthSchemePrefixes.Service}|{AuthSchemePrefixes.Tests}|{AuthSchemePrefixes.User}):\\/\\/.+$")
                    == false,
                    "Identity Chain header invalid");

                var lastIdentityScheme = message.Headers.GetIdentityChain().SubstringBeforeLast("://");
                var lastIdentityValue = message.Headers.GetIdentityChain().SubstringAfterLast("://");

                switch (lastIdentityScheme)
                {
                    case AuthSchemePrefixes.Service:

                        var appId = AesOps.Decrypt(lastIdentityValue, applicationConfig.EncryptionKey);
                        Guard.Against(applicationConfig.AppId != appId, "access token invalid");

                        identityPermissions = new IdentityPermissions
                        {
                            ApiPermissions = allApiPermissions()
                            //TODO Databasepermissions = allDbPermissions()
                        };

                        //* user profile remains null 
                        break;

                    case AuthSchemePrefixes.Tests:
                        var idToken = AesOps.Decrypt(message.Headers.GetIdentityToken(), applicationConfig.EncryptionKey);

                        break;
                    case AuthSchemePrefixes.User:

                        identityPermissions = await Auth0Functions.GetPermissionsFromAccessToken(
                                                  applicationConfig,
                                                  message.Headers.GetAccessToken(),
                                                  securityInfo);

                        userProfile = await Auth0Functions.Profiles.GetUserProfile<TUserProfile>(
                                          applicationConfig,
                                          dataStore,
                                          message.Headers.GetIdentityToken());
                        break;
                }
            }

            return (identityPermissions, userProfile);

            List<string> allApiPermissions() =>
                message.GetType()
                       .Assembly.GetTypes()
                       .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                       .Select(t => t.Name)
                       .Union(
                           typeof(MessageFailedAllRetries).Assembly.GetTypes()
                                                          .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                          .Select(t => t.Name))
                       .ToList();

            static bool IsSubjectToAuthorisation(ApiMessage m, IBootstrapVariables bootstrapVariables)
            {
                var messageType = m.GetType();

                return bootstrapVariables.AuthEnabled && messageType.InheritsOrImplements(typeof(ApiCommand))
                                                      && !messageType.HasAttribute<AuthorisationNotRequired>();
            }
        }

        //it's possible in the future this could turn into an expensive operation that's why we have placed the cache
        public static Task<ServiceLevelAuthority> GetServiceLevelAuthority(IBootstrapVariables bootstrapVariables)
        {
            cache ??= new ServiceLevelAuthority
            {
                IdentityChainSegment = $"{AuthSchemePrefixes.Service}://" + bootstrapVariables.AppId,
                AccessToken = RandomOps.NewString(64),
                IdentityToken = AesOps.Encrypt(bootstrapVariables.AppId, bootstrapVariables.EncryptionKey)
            };

            return Task.FromResult(cache);
        }
    }
}
