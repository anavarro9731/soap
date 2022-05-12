namespace Soap.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using Microsoft.IdentityModel.Tokens;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    /* we could store the users role info in cosmos rather than in the metadata of the IDAAM provider
     but I think then there is a risk that we could have a transactional integrity problem since auth0 */
    public class IdaamProvider : IIdaamProvider
    {


        public class AppMetaData
        {
            public List<RoleInstance> Roles {get; set; }
        }
        
        private readonly ApplicationConfig config;
        private readonly DataStore dataStore;

        public IdaamProvider(ApplicationConfig config, DataStore dataStore)
        {

            this.config = config;
            this.dataStore = dataStore;            
        }

                    /* gets or creates a matching user profile record in the localdb if none exists
             updates the user's profile if details from idaam provider have changed since this method
             was last called */
            public async Task<TUserProfile> GetOrAddUserProfile<TUserProfile>(
                ApplicationConfig applicationConfig,
                DataStore dataStore,
                string idToken) where TUserProfile : class, IHaveAuth0Id, IUserProfile, IAggregate, new()
            {
                Guard.Against(idToken == null,"idToken parameter cannot be null");

                var auth0User = await GetLimitedUserProfileFromIdentityToken(idToken, applicationConfig);

                var user = (await dataStore.Read<TUserProfile>(x => x.Auth0Id == auth0User.UserId)).SingleOrDefault();

                if (user == null)
                {
                    var newUser = new TUserProfile
                    {
                        Auth0Id = auth0User.UserId,
                        Email = auth0User.Email,
                        FirstName = FirstName(auth0User),
                        LastName = LastName(auth0User)
                    };

                    return await dataStore.Create(newUser);
                }

                return (await dataStore.UpdateWhere<TUserProfile>(
                            u => u.Auth0Id == user.Auth0Id,
                            x =>
                                {
                                x.Email = auth0User.Email;
                                x.FirstName = FirstName(auth0User);
                                x.LastName = LastName(auth0User);
                                })).Single();

                string FirstName(User auth0User) =>
                    !string.IsNullOrEmpty(auth0User.FirstName)
                        ? auth0User.FirstName
                        : (auth0User.FullName.Contains(' ') ? auth0User.FullName.SubstringBeforeLast(' ') : auth0User.FullName);

                string LastName(User auth0User) =>
                    !string.IsNullOrEmpty(auth0User.LastName) ? auth0User.LastName : auth0User.FullName.SubstringAfterLast(' ');
             
                
                //* if we can get everything we need by decrypting the token, using method GetLimitedUserProfileFromIdentityToken instead, we will do it that way to avoid the http call, especially if we'd have to do it on every azf call
            static async Task<User> GetUserProfileFromIdentityServer(ApplicationConfig applicationConfig, string auth0Id)
            {
                ManagementApiClient client = null;

                await Auth0Functions.Internal.GetManagementApiClientCached(applicationConfig, v => client = v);

                var user = await client.Users.GetAsync(auth0Id);

                return user;
            }

            static async Task<User> GetLimitedUserProfileFromIdentityToken(string idToken, ApplicationConfig applicationConfig)
            {
                var openIdConfig = await GetOpenIdConfig($"{applicationConfig.Auth0TenantDomain}");

                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidIssuer = openIdConfig.Issuer
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    //* will validate formation and signature by default

                    tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);
                    var tokenWithClaims = validatedToken as JwtSecurityToken;

                    return new User
                    {
                        Email = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "email")?.Value,
                        FirstName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "given_name")?.Value,
                        LastName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "family_name")?.Value,
                        FullName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "name")?.Value,
                        NickName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "nickname")?.Value,
                        UserId = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "sub")?.Value
                    };
                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new ApplicationException("The id token is expired.", ex);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("The id token is invalid.", e);
                }
            }
                
            }

        
        public Task AddRoleToUser(string idaamProviderUserId, RoleId roleId)
        {
            return AddRoleToUser(idaamProviderUserId, roleId, (DatabaseScopeReference)null);
        }

        public Task AddRoleToUser(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, roleId, new List<DatabaseScopeReference>(){scopeReferenceToAdd});
        }

        public async Task AddRoleToUser(string idaamProviderUserId, RoleId roleId, List<DatabaseScopeReference> scopeReferencesToAdd)
        {
            var client = await GetCachedApiClient();
            var user = await client.Users.GetAsync(idaamProviderUserId);
            AppMetaData appMetaData = user.AppMetadata ?? new AppMetaData();

            //*add if completely new
            var existingRole = appMetaData.Roles.SingleOrDefault(x => x.RoleId == roleId);
            if (existingRole==null)
            {
                appMetaData.Roles.Add(
                    new RoleInstance()
                    {
                        RoleId = roleId,
                        ScopeReferences = scopeReferencesToAdd
                    });
            } else //* merge role data
            {
                var existingReferences = existingRole.ScopeReferences;
                foreach (var scopeReference in scopeReferencesToAdd.Where(scopeReference => !existingReferences.Contains(scopeReference)))
                {
                    existingReferences.Add(scopeReference);
                }
            }
            
            var result = await client.Users.UpdateAsync(idaamProviderUserId, new UserUpdateRequest()
            {
                    AppMetadata = appMetaData //TODO check if this merges at the right level i.e. replacing appMetaData.Roles each time
            });
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, RoleId roleId, List<DatabaseScopeReference> scopeReferencesToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, roleId, scopeReferencesToAdd);
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToAdd)
        {
            return  AddRoleToUser(idaamProviderUserId, roleId, scopeReferenceToAdd);
        }
        
        public async Task<string> AddUser(AddUserArgs args)
        {
            var client = await GetCachedApiClient();
            
            var result = await client.Users.CreateAsync(new UserCreateRequest
            {
                FirstName = args.FirstName,
                LastName = args.LastName,
                FullName = $"{args.FirstName} {args.LastName}",
                Password = args.Password,
                Email = args.Email,
                EmailVerified = args.EmailVerified,
                Blocked = args.Blocked,
                VerifyEmail = args.VerifyEmail,
                Connection = this.config.Auth0NewUserConnection,
            });
            return result.UserId;
        }

        public async Task RemoveRoleFromUser(string idaamProviderUserId, RoleId roleId)
        {
            var client = await GetCachedApiClient();
            
            var user = await client.Users.GetAsync(idaamProviderUserId);
            AppMetaData appMetaData = user.AppMetadata ?? new AppMetaData();
            
            if (appMetaData.Roles.Exists(x => x.RoleId == roleId))
            {
                appMetaData.Roles = appMetaData.Roles.Where(role => role.RoleId != roleId).ToList();
            }

            await client.Users.UpdateAsync(idaamProviderUserId, new UserUpdateRequest()
            {
                AppMetadata = appMetaData //TODO check if this merges at the right level i.e. replacing appMetaData.Roles each time
            });
        }

        public async Task RemoveScopeFromUserRole(string idaamProviderUserId, RoleId roleId, DatabaseScopeReference scopeReferenceToRemove)
        {
            var client = await GetCachedApiClient();
            var user = await client.Users.GetAsync(idaamProviderUserId);
            AppMetaData appMetaData = user.AppMetadata ?? new AppMetaData();

            Guard.Against(!appMetaData.Roles.Exists(x => x.RoleId == roleId), "User does not have the role the requested change is for");
            appMetaData.Roles.Single(x => x.RoleId == roleId).ScopeReferences.RemoveAll(x => x == scopeReferenceToRemove);
            
            await client.Users.UpdateAsync(idaamProviderUserId, new UserUpdateRequest()
            {
                AppMetadata = appMetaData 
            });
        }

        private async Task<ManagementApiClient> GetCachedApiClient()
        {
            ManagementApiClient client = null;
            
            await Auth0Functions.Internal.GetManagementApiClientCached(this.config, v => client = v );
            
            return client;
        }
    }
}