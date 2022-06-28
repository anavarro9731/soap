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

        public delegate Task SchemeAuth<TUserProfile>(
            IIdaamProvider idaamProvider,
            IApplicationConfig applicationConfig,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityClaims> setClaims,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new();

        public static async Task 
            AuthenticateandAuthoriseOrThrow<TUserProfile>(
            IIdaamProvider idaamProvider,
            ApiMessage message,
            IApplicationConfig applicationConfig,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            Action<IdentityClaims> setClaims,
            Action<IUserProfile> setUserProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            {
                IdentityClaims identityClaimsInternal = null;
                IUserProfile userProfileInternal = null;

                var shouldAuthorise = IsSubjectToAuthorisation(message, applicationConfig);

                /* if you don't authorise the message, you don't attempt to authenticate the user either.
                 however, just because you authenticate doesn't mean you always return a user profile, services and tests don't have them */
                if (shouldAuthorise)
                {
                    Guard.Against(
                        shouldAuthorise && (message.Headers.GetIdentityChain() == null || message.Headers.GetIdentityToken() == null
                                                                                       || message.Headers.GetAccessToken() == null),
                        "All Authorisation headers not provided but message is not exempt from authorisation",
                        "A Security policy violation is preventing this action from succeeding S01");

                    Guard.Against(
                        Regex.IsMatch(
                            message.Headers.GetIdentityChain(),
                            $"^({AuthSchemePrefixes.Service}|{AuthSchemePrefixes.User}):\\/\\/.+$") == false,
                        "Identity Chain header invalid",
                        "A Security policy violation is preventing this action from succeeding S02");

                    var lastIdentityScheme = message.Headers.GetIdentityChain().SubstringBeforeLast("://");
                    if (lastIdentityScheme.Contains("://")) lastIdentityScheme = lastIdentityScheme.SubstringAfterLast(",");
                    var lastIdentityValue = message.Headers.GetIdentityChain().SubstringAfterLast("://");

                    var schemeHandlers = new Dictionary<string, SchemeAuth<TUserProfile>>()
                    {
                        {AuthSchemePrefixes.Service, ServiceSchemeAuth<TUserProfile> },
                        {AuthSchemePrefixes.User, UserSchemeAuth<TUserProfile> }
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
                        v => userProfileInternal = v);

                    if (message is MessageFailedAllRetries m)
                    {
                        message.Validate();
                        var nameOfFailedMessage = Type.GetType(m.TypeName).Name;

                        Guard.Against(
                            identityClaimsInternal == null || !identityClaimsInternal.ApiPermissions.SelectMany(x => x.DeveloperPermissions).Contains(nameOfFailedMessage),
                            AuthErrorCodes.NoApiPermissionExistsForThisMessage,
                            "A Security policy violation is preventing this action from succeeding S04");
                    }
                    else
                    {
                        Guard.Against(
                            identityClaimsInternal == null || !identityClaimsInternal.ApiPermissions.SelectMany(x => x.DeveloperPermissions).Contains(message.GetType().Name),
                            AuthErrorCodes.NoApiPermissionExistsForThisMessage,
                            "A Security policy violation is preventing this action from succeeding S04");
                    }
                }

                await SaveOrUpdateUserProfileInDb(userProfileInternal as TUserProfile, dataStore);

                //* if auth is enabled these could be empty but should never be null
                setClaims(identityClaimsInternal);
                setUserProfile(userProfileInternal);
            }

            static bool IsSubjectToAuthorisation(ApiMessage m, IApplicationConfig applicationConfig)
            {
                var messageType = m.GetType();

                return applicationConfig.AuthLevel.ApiPermissionEnabled && !messageType.HasAttribute<AuthorisationNotRequired>();
            }

            static async Task SaveOrUpdateUserProfileInDb<TUserProfileMethodLevel>(TUserProfileMethodLevel userProfile, DataStore dataStore)
                where TUserProfileMethodLevel : class, IUserProfile, IAggregate, new()
            {
                if (userProfile == null) return;

                var user = (await dataStore.Read<TUserProfileMethodLevel>(x => x.IdaamProviderId == userProfile.IdaamProviderId)).SingleOrDefault();

                if (user == null)
                {
                    var newUser = new TUserProfileMethodLevel
                    {
                        id = userProfile.id,
                        IdaamProviderId = userProfile.IdaamProviderId,
                        Email = userProfile.Email,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName
                    };

                    await dataStore.Create(newUser);
                }
                else
                {
                    await dataStore.UpdateById<TUserProfileMethodLevel>(
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
                    IdentityToken = AesOps.Encrypt(bootstrapVariables.AppId, bootstrapVariables.EncryptionKey)
                };
            }

            return cache;
        }

        public static Task ServiceSchemeAuth<TUserProfile>( //* gives you root access
            IIdaamProvider idaamProvider,
            IBootstrapVariables bootstrapVariables,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue,
            Action<IdentityClaims> setClaims,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            
            var appId = AesOps.Decrypt(message.Headers.GetIdentityToken(), bootstrapVariables.EncryptionKey);
            Guard.Against(schemeValue != appId, "last scheme value should match decrypted id token");
            Guard.Against(bootstrapVariables.AppId != appId, "access token should match app id");

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

        public static async Task UserSchemeAuth<TUserProfile>(
            IIdaamProvider idaamProvider,
            IBootstrapVariables bootstrapVariables,
            ApiMessage message,
            DataStore dataStore,
            ISecurityInfo securityInfo,
            string schemeValue, //* currently not used for securing request with user scheme, apart from visual debug, if this changes consider how it is set by various clients
            Action<IdentityClaims> setClaims,
            Action<IUserProfile> setProfile) where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            var accessToken = message.Headers.GetAccessToken();
            var idToken = message.Headers.GetIdentityToken();

            var identityClaims = await idaamProvider.GetAppropriateClaimsFromAccessToken(accessToken, message, securityInfo);

            setClaims(identityClaims);

            var userProfile = idToken == null ? null : await RefreshAndReturnOrAddUserProfile(dataStore, idaamProvider, idToken);

            setProfile(userProfile);
            

        static async Task<TUserProfile> RefreshAndReturnOrAddUserProfile(DataStore dataStore, IIdaamProvider idaamProvider, string idToken)
        {
            /* gets or creates a matching user profile record in the localdb if none exists
             updates the user's profile if details from idaam provider have changed since this method
             was last called */

            Guard.Against(idToken == null, "idToken parameter cannot be null");

            var idaamUser = await idaamProvider.GetLimitedUserProfileFromIdentityToken(idToken);

            var user = (await dataStore.Read<TUserProfile>(x => x.IdaamProviderId == idaamUser.IdaamProviderId)).SingleOrDefault();

            if (user == null)
            {
                var newUser = new TUserProfile
                {
                    IdaamProviderId = idaamUser.IdaamProviderId,
                    Email = idaamUser.Email,
                    FirstName = idaamUser.FirstName,
                    LastName = idaamUser.LastName
                };

                return await dataStore.Create(newUser);
            }

            return (await dataStore.UpdateWhere<TUserProfile>(
                        u => u.IdaamProviderId == user.IdaamProviderId,
                        x =>
                            {
                            x.Email = idaamUser.Email;
                            x.FirstName = idaamUser.FirstName;
                            x.LastName = idaamUser.LastName;
                            })).Single();
            
        }
        }
    }
}