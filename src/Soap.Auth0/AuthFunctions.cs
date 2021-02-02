namespace Soap.Context
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Soap.Auth0;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class AuthFunctions
    {
        private static ServiceLevelAuthority cache;

        public delegate Task SchemeAuth<TUserProfile>(
            ApplicationConfig applicationConfig,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityPermissions> setPermissions,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new();

        public static async Task AuthenticateandAuthoriseOrThrow<TUserProfile>(
            ApiMessage message,
            ApplicationConfig applicationConfig,
            DataStore dataStore,
            Dictionary<string, SchemeAuth<TUserProfile>> schemeHandlers,
            ISecurityInfo securityInfo,
            Action<IdentityPermissions> setPermissions,
            Action<IUserProfile> setUserProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            {
                IdentityPermissions identityPermissions = null;
                TUserProfile userProfile = null;
                
                var shouldAuthorise = IsSubjectToAuthorisation(message, applicationConfig);

                /* if you don't authorise the message, you don't attempt to authenticate the user either.
                 however, just because you authenticate doesn't mean you always return a user profile, services and tests don't have them */
                if (shouldAuthorise)
                {
                    Guard.Against(
                        shouldAuthorise && (message.Headers.GetIdentityChain() == null
                                            || message.Headers.GetIdentityToken() == null
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

                    Guard.Against(
                        !schemeHandlers.ContainsKey(lastIdentityScheme),
                        "Could not find a handler to process the identity scheme " + lastIdentityScheme);

                    var handler = schemeHandlers[lastIdentityScheme];

                    handler(
                        applicationConfig,
                        message,
                        dataStore,
                        securityInfo,
                        lastIdentityValue,
                        setPermissions,
                        setUserProfile);
                }

                await SaveOrUpdateUserProfileInDb(userProfile, dataStore);
                
                setPermissions(identityPermissions);
                setUserProfile(userProfile);
            }

            static bool IsSubjectToAuthorisation(ApiMessage m, IBootstrapVariables bootstrapVariables)
            {
                var messageType = m.GetType();

                return bootstrapVariables.AuthEnabled && messageType.InheritsOrImplements(typeof(ApiCommand))
                                                      && !messageType.HasAttribute<AuthorisationNotRequired>();
            }

            static async Task SaveOrUpdateUserProfileInDb<TUserProfile>(TUserProfile userProfile, DataStore dataStore)
                where TUserProfile : class, IUserProfile, IAggregate, new()
            {
                if (userProfile == null) return;

                var user = (await dataStore.Read<TUserProfile>(x => x.Auth0Id == userProfile.Auth0Id)).SingleOrDefault();

                if (user == null)
                {
                    var newUser = new TUserProfile
                    {
                        Auth0Id = userProfile.Auth0Id,
                        Email = userProfile.Email,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName
                    };

                    await dataStore.Create(newUser);
                }

                await dataStore.UpdateWhere<TUserProfile>(
                    u => u.Auth0Id == user.Auth0Id,
                    x =>
                        {
                        x.Email = userProfile.Email;
                        x.FirstName = userProfile.FirstName;
                        x.LastName = userProfile.LastName;
                        });
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

        public static async Task ServiceSchemeAuth<TUserProfile>(
            ApplicationConfig applicationConfig,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityPermissions> setPermissions,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            var appId = AesOps.Decrypt(schemeValue, applicationConfig.EncryptionKey);
            Guard.Against(applicationConfig.AppId != appId, "access token invalid");

            var identityPermissions = new IdentityPermissions
            {
                ApiPermissions = GetAllApiPermissions(message)
                //TODO Databasepermissions = allDbPermissions()
            };
            setPermissions(identityPermissions);
            //* user profile remains null
        }

        public static async Task UserSchemeAuth<TUserProfile>(
            ApplicationConfig applicationConfig,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityPermissions> setPermissions,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {

            var accessToken = message.Headers.GetAccessToken();
            var idToken = message.Headers.GetIdentityToken();
                
            var identityPermissions = await Auth0Functions.GetPermissionsFromAccessToken(
                                          applicationConfig,
                                          accessToken,
                                          securityInfo);

            setPermissions(identityPermissions);

            var userProfile = await Auth0Functions.Profiles.GetUserProfileOrNull<TUserProfile>(
                                  applicationConfig,
                                  dataStore,
                                  idToken);

            setProfile(userProfile);
        }

        private static List<string> GetAllApiPermissions(ApiMessage message) =>
            message.GetType()
                   .Assembly.GetTypes()
                   .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                   .Select(t => t.Name)
                   .Union(
                       typeof(MessageFailedAllRetries).Assembly.GetTypes()
                                                      .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                      .Select(t => t.Name))
                   .ToList();
    }
}
