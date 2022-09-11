namespace Soap.Idaam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class AuthorisationSchemes
    {
        private static ServiceLevelAuthority cache;

        private static object cacheLock = new object();

        public delegate Task SchemeAuth(
            IIdaamProvider idaamProvider,
            IApplicationConfig applicationConfig,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityClaims> setClaims,
            Action<IdaamProviderProfile> setProfile);

        public static async Task 
            AuthenticateandAuthoriseOrThrow<TUserProfile>(
            IIdaamProvider idaamProvider,
            ApiMessage message,
            IApplicationConfig applicationConfig,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            Action<IdentityClaims> setClaims,
            Action<TUserProfile> setUserProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            {
                IdentityClaims identityClaimsInternal = null;
                IdaamProviderProfile idaamProfile = null;

                var shouldAuthenticate = IsSubjectToAuthentication(message, applicationConfig);
                var shouldAuthorise = IsSubjectToAuthorisation(message, applicationConfig);

                /* if you don't authenticate the message, you don't attempt to authorise the user either.
                 however, just because you authenticate doesn't mean you always return a user profile, service users dont have them */
                if (shouldAuthenticate)
                {
                    Guard.Against(
                        (message.Headers.GetIdentityToken() == null),
                        "Required Authorisation headers not provided but message is not exempt from authentication",
                        "A Security policy violation is preventing this action from succeeding S06");
                    
                    Guard.Against( message.Headers.GetIdentityChain() == null ||
                                  !Regex.IsMatch(
                                      message.Headers.GetIdentityChain(),
                                      $"^({AuthSchemePrefixes.Service}|{AuthSchemePrefixes.User}):\\/\\/.+$"),
                        "Identity Chain header invalid",
                        "A Security policy violation is preventing this action from succeeding S02");

                    var lastIdentityScheme = message.Headers.GetIdentityChain().SubstringBeforeLast("://");
                    if (lastIdentityScheme.Contains("://")) lastIdentityScheme = lastIdentityScheme.SubstringAfterLast(",");
                    var lastIdentityValue = message.Headers.GetIdentityChain().SubstringAfterLast("://");

                    var schemeHandlers = new Dictionary<string, SchemeAuth>()
                    {
                        {AuthSchemePrefixes.Service, ServiceSchemeAuth },
                        {AuthSchemePrefixes.User, UserSchemeAuth }
                    };
                                                                 
                    Guard.Against(
                        !schemeHandlers.ContainsKey(lastIdentityScheme),
                        "Could not find a handler to process the identity scheme " + lastIdentityScheme,
                        "A Security policy violation is preventing this action from succeeding S03");

                    var schemeHandler = schemeHandlers[lastIdentityScheme];

                    await schemeHandler(
                        idaamProvider,
                        applicationConfig,
                        message,
                        dataStore,
                        securityInfo,
                        lastIdentityValue,
                        v => identityClaimsInternal = v,
                        v => idaamProfile = v);

                    if (shouldAuthorise)
                    {
                        Guard.Against(
                            (message.Headers.GetAccessToken() == null),
                            "Required Authorisation headers not provided but message is not exempt from authentication",
                            "A Security policy violation is preventing this action from succeeding S01");
                        
                        if (message is MessageFailedAllRetries m)
                        {
                            message.Validate();
                            var nameOfFailedMessage = Type.GetType(m.TypeName).Name;

                            Guard.Against(
                                identityClaimsInternal == null || !identityClaimsInternal.ApiPermissions.SelectMany(x => x.DeveloperPermissions)
                                                                                         .Contains(nameOfFailedMessage),
                                AuthErrorCodes.NoApiPermissionExistsForThisMessage,
                                "A Security policy violation is preventing this action from succeeding S04");
                        }
                        else
                        {
                            Guard.Against(
                                identityClaimsInternal == null || !identityClaimsInternal.ApiPermissions.SelectMany(x => x.DeveloperPermissions)
                                                                                         .Contains(message.GetType().Name),
                                AuthErrorCodes.NoApiPermissionExistsForThisMessage,
                                "A Security policy violation is preventing this action from succeeding S05");
                        }
                    }
                }

                var userProfile = await SaveOrUpdateUserProfileInDb<TUserProfile>(idaamProfile, dataStore);

                //* if auth is enabled these could be empty but should never be null
                setClaims(identityClaimsInternal);
                setUserProfile(userProfile);
            }

            static bool IsSubjectToAuthentication(ApiMessage m, IApplicationConfig applicationConfig)
            {
                var messageType = m.GetType();

                var messageIsExempt = messageType.HasAttribute<AuthenticationNotRequired>();
                
                return applicationConfig.AuthLevel.AuthenticationRequired && !messageIsExempt;
            }
            
            static bool IsSubjectToAuthorisation(ApiMessage m, IApplicationConfig applicationConfig)
            {
                var messageType = m.GetType();

                var messageIsExempt = messageType.HasAttribute<AuthorisationNotRequired>();
                
                return applicationConfig.AuthLevel.ApiPermissionsRequired && !messageIsExempt;
            }

            static async Task<TUserProfileMethodLevel> SaveOrUpdateUserProfileInDb<TUserProfileMethodLevel>(IdaamProviderProfile userProfile, DataStore dataStore)
                where TUserProfileMethodLevel : class, IUserProfile, IAggregate, new()
            {
                if (userProfile == null) return null;

                var user = (await dataStore.Read<TUserProfileMethodLevel>(x => x.IdaamProviderId == userProfile.IdaamProviderId)).SingleOrDefault();

                if (user == null)
                {
                    var newUser = new TUserProfileMethodLevel
                    {
                        id = Guid.NewGuid(),
                        IdaamProviderId = userProfile.IdaamProviderId,
                        Email = userProfile.Email,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName,
                        
                    };

                    return await dataStore.Create(newUser);
                }
                else
                {
                    return await dataStore.UpdateById<TUserProfileMethodLevel>(
                        user.id,
                        x =>
                            {
                            x.Email = userProfile.Email;
                            x.FirstName = userProfile.FirstName;
                            x.LastName = userProfile.LastName;
                            });
                }
            }
        }

        //it's possible in the future this could turn into an expensive operation that's why we have placed the cache
        public static ServiceLevelAuthority GetServiceLevelAuthority(IBootstrapVariables bootstrapVariables)
        {
            lock (cacheLock)
            {
                cache ??= new ServiceLevelAuthority
                {
                    IdentityChainSegment = $"{AuthSchemePrefixes.Service}://" + bootstrapVariables.AppId,
                    AccessToken = RandomOps.RandomString(64),
                    IdentityToken = AesOps.Encrypt($"{bootstrapVariables.AppId}.sla", bootstrapVariables.EncryptionKey)
                };
            }

            return cache;
        }

        public static Task ServiceSchemeAuth( //* gives you root access
            IIdaamProvider idaamProvider,
            IBootstrapVariables bootstrapVariables,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string identityValueWhichIsServiceName,
            Action<IdentityClaims> setClaims,
            Action<IdaamProviderProfile> setProfile) 
        {
            
            var idTokenWhichHasTheServiceName = AesOps.Decrypt(message.Headers.GetIdentityToken(), bootstrapVariables.EncryptionKey);
            Guard.Against("sla" != idTokenWhichHasTheServiceName.SubstringAfter('.'), "Service token identities do not match");
            

            var allRoles = securityInfo.BuiltInRoles.Select(
                                           x => new RoleInstance()
                                           {
                                               RoleKey = x.Key
                                           })
                                       .ToList();

            
            var identityPermissions = new IdentityClaims
            {
                Roles = allRoles,
                ApiPermissions = securityInfo.ApiPermissions,
                DatabasePermissions  = new List<DatabasePermission>()
                {
                    //* by using the wildcard here, you don't any scopes on the role
                    new DatabasePermission("*", null)
                }
            };
            setClaims(identityPermissions);
            
            setProfile(null); //* user profile remains null
            
            return Task.CompletedTask;
        }

        public static async Task UserSchemeAuth(
            IIdaamProvider idaamProvider,
            IBootstrapVariables bootstrapVariables,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue, //* currently not used for securing request with user scheme, apart from visual debug, if this changes consider how it is set by various clients
            Action<IdentityClaims> setClaims,
            Action<IdaamProviderProfile> setProfile) 
        {
            var accessToken = message.Headers.GetAccessToken();
            var idToken = message.Headers.GetIdentityToken();

            var userProfile = await idaamProvider.GetLimitedUserProfileFromIdentityToken(idToken);
            
            setProfile(userProfile);
            
            var identityClaims = await idaamProvider.GetAppropriateClaimsFromAccessToken(accessToken, userProfile.IdaamProviderId, message, securityInfo);

            setClaims(identityClaims);

            
            
        }
    }
}