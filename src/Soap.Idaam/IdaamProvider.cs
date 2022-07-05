namespace Soap.Idaam
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using global::Auth0.ManagementApi.Paging;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Enums;
    using Soap.Utility.Functions.Extensions;
    using Role = Soap.Interfaces.Role;

    /* we could store the users role info in cosmos rather than in the metadata of the IDAAM provider
     but I think then there is a risk that we could have a transactional integrity problem since auth0 
     cannot enlist in the txn */
    public class IdaamProvider : IIdaamProvider
    {
        private static (ManagementApiClient apiClient, DateTime expires) managementApiClientCache;

        private static OpenIdConnectConfiguration openIdConnectConfigurationCache;

        private static RestClient restClientCache;

        private readonly ApplicationConfig config;

        public IdaamProvider(ApplicationConfig config)
        {
            this.config = config;
        }
        
        public async Task<string> GetUiApplicationClientId(Assembly messagesAssembly)
        {
            {
                var client = await GetManagementApiClientCached();
        
                GetApiName(this.config, out var apiName);
        
                GetUiAppName(apiName, out string appName);
        
                var appClientId = await ProcessPageOfApiClientApplicationResults(client, 0, appName);
        
                return appClientId;
            }
        
            static async Task<string> ProcessPageOfApiClientApplicationResults(ManagementApiClient client, int pageNo, string appName)
            {
                var page = await client.Clients.GetAllAsync(
                               new GetClientsRequest
                               {
                                   IsFirstParty = true,
                                   AppType = new[]
                                   {
                                       ClientApplicationType.Spa
                                   },
                                   IncludeFields = true,
                                   Fields = "client_id,name"
                               },
                               new PaginationInfo(pageNo, 50, true));
                if (page.Any(app => app.Name == appName))
                {
                    return page.Single(app => app.Name == appName).ClientId;
                }
        
                var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                return thereAreMoreItems switch
                {
                    true => await ProcessPageOfApiClientApplicationResults(client, ++pageNo, appName),
                    _ => null
                };
            }
        }

        public Task AddRoleToUser(string idaamProviderUserId, Role role)
        {
            return AddRoleToUser(idaamProviderUserId, role, (AggregateReference)null);
        }

        public Task AddRoleToUser(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd)
        {
            if (scopeReferenceToAdd != null)
            {
                return AddRoleToUser(idaamProviderUserId, role, new List<AggregateReference>() { scopeReferenceToAdd });
            }

            return AddRoleToUser(idaamProviderUserId, role, (List<AggregateReference>)null);
        }

        public async Task AddRoleToUser(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd)
        {
            var client = await GetManagementApiClientCached();
            
            var user = await client.Users.GetAsync(idaamProviderUserId);
            
            AppMetaData appMetaData = ((JObject)user.AppMetadata)?.ToObject<AppMetaData>() ?? new AppMetaData();

            scopeReferencesToAdd ??= new List<AggregateReference>();
            
            //*add if completely new
            var existingRole = appMetaData.Roles.SingleOrDefault(x => x.RoleKey == role.Key);
            if (existingRole == null)
            {
                appMetaData.Roles.Add(
                    new RoleInstance()
                    {
                        RoleKey = role.Key,
                        ScopeReferences = scopeReferencesToAdd
                    });
            }
            else //* merge role data
            {
                var existingReferences = existingRole.ScopeReferences;
                foreach (var scopeReference in scopeReferencesToAdd.Where(scopeReference => !existingReferences.Contains(scopeReference)))
                {
                    existingReferences.Add(scopeReference);
                }
            }

            //* add our "RoleInstance" to the metadata which contains the scope(s) of data this role applicable to
            var result = await client.Users.UpdateAsync(
                             idaamProviderUserId,
                             new UserUpdateRequest()
                             {
                                 AppMetadata = appMetaData 
                             });

            //* rate limiting!
            var getRoleId = GetRoleId(client, role.AsAuth0Name(EnvVars.EnvironmentPartitionKey));
            string roleId = await getRoleId;
            
            await client.Users.AssignRolesAsync(
                idaamProviderUserId,
                new AssignRolesRequest()
                {
                    Roles = new[] { roleId }
                });

        }
        
        //* assign the Auth0 role
        static async Task<string> GetRoleId(ManagementApiClient client, string roleName)
        {
            var page = await client.Roles.GetAllAsync(
                           new GetRolesRequest()
                           {
                               NameFilter = roleName
                           });
                
            Guard.Against(page.Count > 1, "Duplicate RoleIds", ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                
            return page.Single().Id;
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, Role role, List<AggregateReference> scopeReferencesToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, role, scopeReferencesToAdd);
        }

        public Task AddScopeToUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToAdd)
        {
            return AddRoleToUser(idaamProviderUserId, role, scopeReferenceToAdd);
        }

        public async Task<string> AddUser(IIdaamProvider.AddUserArgs args)
        {
            var client = await GetManagementApiClientCached();

            var result = await client.Users.CreateAsync(
                             new UserCreateRequest
                             {
                                 FirstName = args.Profile.FirstName,
                                 LastName = args.Profile.LastName,
                                 FullName = $"{args.Profile.FirstName} {args.Profile.LastName}",
                                 Password = args.Password,
                                 Email = args.Profile.Email,
                                 EmailVerified = args.EmailVerified,
                                 Blocked = args.Blocked,
                                 VerifyEmail = args.VerifyEmail,
                                 Connection = this.config.Auth0NewUserConnection,
                             });
            
            return result.UserId;
        }

        public async Task ChangeUserPassword(string idaamProviderId, string newPassword)
        {
            var client = await GetManagementApiClientCached();

            await client.Users.UpdateAsync(idaamProviderId,
                new UserUpdateRequest()
                {
                    Password = newPassword
                });
        }
        
        public async Task RemoveUser(string idaamProviderId)
        {
            var client = await GetManagementApiClientCached();

            await client.Users.DeleteAsync(idaamProviderId);
            
        }
        
        public async Task<string> BlockUser(string idaamProviderId)
        {
            var client = await GetManagementApiClientCached();

            var result = await client.Users.UpdateAsync(
                             idaamProviderId,
                             new UserUpdateRequest()
                             {
                                 Blocked = true
                             });
            return result.UserId;
        }

        /* if we can get everything we need by decrypting the token, using method GetLimitedUserProfileFromIdentityToken instead,
        we will do it that way to avoid the http call, especially if we'd have to do it on every azf call, if not we might have to do this */
        public async Task<User> GetUserProfileFromIdentityServer(string idaamProviderId)
        {
            ManagementApiClient client = await GetManagementApiClientCached();

            var user = await client.Users.GetAsync(idaamProviderId);

            return user;
        }
        
        public async Task<IIdaamProvider.User> GetLimitedUserProfileFromIdentityToken(string idToken)
            {
                var openIdConfig = await GetOpenIdConfig($"{this.config.Auth0TenantDomain}");

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

                    var firstName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "given_name")?.Value;
                    var lastName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "family_name")?.Value;
                    var fullName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "name")?.Value;
                    
                    return new IIdaamProvider.User
                    {
                        Email = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "email")?.Value,
                        FirstName = FirstName(firstName, fullName),
                        LastName = LastName(lastName, fullName),
                        IdaamProviderId = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "sub")?.Value
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
                
                string FirstName(string firstName, string fullName) =>
                    !string.IsNullOrEmpty(firstName)
                        ? firstName
                        : (fullName.Contains(' ') ? fullName.SubstringBeforeLast(' ') : fullName);

                string LastName(string lastName, string fullName) =>
                    !string.IsNullOrEmpty(lastName) ? lastName : fullName.SubstringAfterLast(' ');
            }

        public async Task<bool> IsApiRegisteredWithProvider()
        {
            GetApiId(this.config, out string apiId);
            
            var client = await GetManagementApiClientCached();
            
            var exists = await ProcessPageOfApiServerResults(client, 0, apiId);
            
            return exists;

            static async Task<bool> ProcessPageOfApiServerResults(ManagementApiClient client, int pageNo, string apiId)
            {
                var page = await client.ResourceServers.GetAllAsync(new PaginationInfo(pageNo, 50, true));
                if (page.Any(server => server.Identifier == apiId))
                {
                    return true;
                }

                var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                return thereAreMoreItems switch
                {
                    true => await ProcessPageOfApiServerResults(client, ++pageNo, apiId),
                    _ => false
                };
            }
        }

        public async Task RemoveRoleFromUser(string idaamProviderUserId, Role roleToRemove)
        {
            var client = await GetManagementApiClientCached();
            
            
            var user = await client.Users.GetAsync(idaamProviderUserId);
            AppMetaData appMetaData = ((JObject)user.AppMetadata)?.ToObject<AppMetaData>() ?? new AppMetaData();

            if (appMetaData.Roles.Exists(x => x.RoleKey == roleToRemove.Key))
            {
                appMetaData.Roles = appMetaData.Roles.Where(role => role.RoleKey != roleToRemove.Key).ToList();
            }

            await client.Users.UpdateAsync(
                idaamProviderUserId,
                new UserUpdateRequest()
                {
                    AppMetadata = appMetaData 
                });

            
            //* rate limiting!
            var getRoleId = GetRoleId(client, roleToRemove.AsAuth0Name(EnvVars.EnvironmentPartitionKey));
            var roleId = await getRoleId;
            
            await client.Users.RemoveRolesAsync(
                idaamProviderUserId,
                new AssignRolesRequest()
                {
                    Roles = new[] { roleId }
                });
        }

        public async Task RemoveScopeFromUserRole(string idaamProviderUserId, Role role, AggregateReference scopeReferenceToRemove)
        {
            var client = await GetManagementApiClientCached();
            var user = await client.Users.GetAsync(idaamProviderUserId);
            AppMetaData appMetaData = ((JObject)user.AppMetadata)?.ToObject<AppMetaData>() ?? new AppMetaData();

            Guard.Against(!appMetaData.Roles.Exists(x => x.RoleKey == role.Key), "User does not have the role the requested change is for");
            appMetaData.Roles.Single(x => x.RoleKey == role.Key).ScopeReferences.RemoveAll(x => x == scopeReferenceToRemove);

            await client.Users.UpdateAsync(
                idaamProviderUserId,
                new UserUpdateRequest()
                {
                    AppMetadata = appMetaData
                });
        }
        
        
        public async Task<IdentityClaims> GetAppropriateClaimsFromAccessToken(string bearerToken, ApiMessage apiMessage, ISecurityInfo securityInfo)
        {
            {
                var openIdConfig = await GetOpenIdConfig($"{this.config.Auth0TenantDomain}");
                
                GetApiId(this.config, out string apiId);
                
                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidAudience = apiId,
                    ValidateAudience = true,
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

                    var principal = tokenHandler.ValidateToken(bearerToken, validationParameters, out var validatedToken);
                    
                    GetRoles(principal, out IHaveRoles roleContainer);

                    var identityPermissions = ClaimsExtractor.GetAppropriateClaimsFromAccessToken(securityInfo, roleContainer, apiMessage);
                    
                    return identityPermissions;

                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new CircuitException("The access token is expired.", ex);
                }
                catch (Exception e)
                {
                    throw new CircuitException("The access token is invalid.", e);
                }
            }
            

            void GetRoles(ClaimsPrincipal claimsPrincipal, out IHaveRoles roleInstances)
            {
                var claim = claimsPrincipal.Claims.Single(x => x.Type == "https://soap.idaam/app_metadata");
                var jsonAppMetaData = claim.Value;
                var metaData = JsonConvert.DeserializeObject<AppMetaData>(jsonAppMetaData);
                var auth0ApiAssignedRoles = claimsPrincipal.Claims.Where(c => c.Type == "https://soap.idaam/roles" && c.Value.Contains("builtin:"))
                                                           .Select(x => x.Value.SubstringAfter("builtin:"))
                                                           .ToList();
                if (this.config.AuthLevel == AuthLevel.AuthoriseApiPermissions)
                {
                    /* in this case we ignore role metadata and read the auth0 api assigned roles */

                    roleInstances = new AppMetaData()
                    {
                        Roles = auth0ApiAssignedRoles.Select(
                                                   x => new RoleInstance()
                                                   {
                                                       RoleKey = x
                                                   })
                                               .ToList()
                    };
                }
                else
                {
                    roleInstances = metaData;    
                }
                
            }
            
            
        }

        static async Task CreateRoleOnServer(ManagementApiClient client, string apiId, ISecurityInfo securityInfo, Interfaces.Role role)
        {
            var newRole = await client.Roles.CreateAsync(
                              new RoleCreateRequest()
                              {
                                  Description = role.Description ?? role.Value,
                                  Name = role.AsAuth0Name(EnvVars.EnvironmentPartitionKey)
                              });

            if (role.ApiPermissions.Any())
            {
                await client.Roles.AssignPermissionsAsync(
                    newRole.Id,
                    new AssignPermissionsRequest()
                    {
                        Permissions = role.ApiPermissions.Select(
                                              apiPermission => new PermissionIdentity()
                                              {
                                                  Identifier = apiId,
                                                  Name = apiPermission
                                                                     .AsAuth0Claim(EnvVars.EnvironmentPartitionKey)
                                              })
                                          .ToList()
                    });
            }
        }

        static async Task GetApiAccessToken(ApplicationConfig applicationConfig, Action<string> setApiAccessToken, string audience)
        {
            restClientCache ??= new RestClient($"https://{applicationConfig.Auth0TenantDomain}/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddParameter(
                "application/json",
                $"{{\"client_id\":\"{applicationConfig.Auth0EnterpriseAdminClientId}\",\"client_secret\":\"{applicationConfig.Auth0EnterpriseAdminClientSecret}\",\"audience\":\"{audience}\",\"grant_type\":\"client_credentials\"}}",
                ParameterType.RequestBody);
            var response = await restClientCache.ExecuteAsync(request);
            var tokenJson = response.Content;
            dynamic tokenObject = JsonConvert.DeserializeObject(tokenJson);
            string accessToken = null;
            try
            {
                accessToken = tokenObject.access_token;
            }
            catch (RuntimeBinderException)
            {
                throw new CircuitException("Response was invalid when attempting to obtain Auth0 Management Api access token");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new CircuitException("Could not retrieve Auth0 access token for management api, it was blank");
            }

            setApiAccessToken(accessToken);
        }

        static void GetApiName(ApplicationConfig applicationConfig, out string apiName) => apiName = applicationConfig.AppId;

        static string GetUiAppName(string apiName, out string appName) => appName = $"{apiName}.ui";

        public string GetApiClient()
        {
            GetApiId(this.config, out var id);
            return id;
        }
        
        //* this needs to use function app host because the scheme HTTP/HTTPS has to match the environment
        static void GetApiId(ApplicationConfig applicationConfig, out string apiId) => apiId = applicationConfig.FunctionAppHostUrlWithTrailingSlash + applicationConfig.AppId;

        /* admin token for making calls to our API
         depending on your Auth0plan, these can be limited in number you can acquire without incurring extra costs
         last check the free plan only supported 1K/mo this is why we use the internal servicetoken instead but im keeping the function here for reference */
        static async Task<string> GetEnterpriseAdminM2MTokenForThisApi(ApplicationConfig applicationConfig)
        {
            string adminM2MToken = null;
            GetApiId(applicationConfig, out string apiId);
            await GetApiAccessToken(applicationConfig, v => adminM2MToken = v, apiId);

            return adminM2MToken;
        }

        static List<ResourceServerScope> GetLocalApiPermissions(ISecurityInfo securityInfo)
        {
            var scopes = securityInfo.ApiPermissions.Select(
                                         x => new ResourceServerScope
                                         {
                                             Description = x.Description ?? x.Value,
                                             Value = x.AsAuth0Claim(EnvVars.EnvironmentPartitionKey)
                                         })
                                     .ToList();

            return scopes;
        }

        static async Task<OpenIdConnectConfiguration> GetOpenIdConfig(string tenantDomain)
        {
            //* Get the public keys from the jwks endpoint   

            //* cache til static expires in azure functions, as its expensive
            openIdConnectConfigurationCache ??= await GetOpenIdConfigInternal(tenantDomain);
            return openIdConnectConfigurationCache;

            static async Task<OpenIdConnectConfiguration> GetOpenIdConfigInternal(string tenantDomain)
            {
                var openIdConfigurationEndpoint = $"https://{tenantDomain}/.well-known/openid-configuration";
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    openIdConfigurationEndpoint,
                    new OpenIdConnectConfigurationRetriever());
                var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
                return openIdConfig;
            }
        }

        public async Task RegisterApiWithProvider(
            ISecurityInfo securityInfo,
            Func<string, ValueTask> writeLine)
        {
            {
                var client = await GetManagementApiClientCached();
                
                GetApiName(this.config, out string apiName);
                GetUiAppName(apiName, out string appName);
                GetApiId(this.config, out string apiId);

                var resourceServerScopes = GetLocalApiPermissions(securityInfo);

                await writeLine($"Auth0 Api for this service in environment {this.config.Environment.Value} does not exist. Creating it now.");

                await CreateApiWithPermissions(resourceServerScopes, apiName, apiId, client);

                await GrantMachineTokenAccessToApi(resourceServerScopes, client, apiId, this.config);

                await CreateFrontEnd(client, appName, apiName, this.config);

                await CreateRoles(client, apiId, securityInfo);
            }

            //* register api 
            static async Task CreateApiWithPermissions(List<ResourceServerScope> resourceServerScopes, string apiName, string apiId, ManagementApiClient client)
            {
                var r = new ResourceServerCreateRequest
                {
                    Identifier = apiId,
                    Name = apiName,
                    Scopes = resourceServerScopes,
                    TokenDialect = TokenDialect.AccessTokenAuthZ,
                    EnforcePolicies = true, //RBAC ENABLED
                    AllowOfflineAccess = true,
                    SkipConsentForVerifiableFirstPartyClients = true
                };
                await client.ResourceServers.CreateAsync(r);
            }

            //* give enterprise admin access to for inter-service calls
            static async Task GrantMachineTokenAccessToApi(List<ResourceServerScope> resourceServerScopes, ManagementApiClient client, string apiId, ApplicationConfig applicationConfig)
            {
                await client.ClientGrants.CreateAsync(
                    new ClientGrantCreateRequest
                    {
                        Audience = apiId,
                        Scope = resourceServerScopes.Select(x => x.Value).ToList(),
                        ClientId = applicationConfig.Auth0EnterpriseAdminClientId //* enterprise admin clientid
                    });
            }

            //* give frontend access by creating frontend app
            static async Task CreateFrontEnd(ManagementApiClient client, string appName, string apiName, ApplicationConfig applicationConfig)
            {
                var result = await client.Clients.CreateAsync(
                                 new ClientCreateRequest
                                 {
                                     Name = appName,
                                     Description = $"{apiName} front end",
                                     ApplicationType = ClientApplicationType.Spa,
                                     IsFirstParty = true,
                                     Callbacks = new[] { applicationConfig.CorsOrigin },
                                     AllowedLogoutUrls = new[] { applicationConfig.CorsOrigin },
                                     AllowedOrigins = new[] { applicationConfig.CorsOrigin },
                                     WebOrigins = new[] { applicationConfig.CorsOrigin },
                                     JwtConfiguration = new JwtConfiguration
                                     {
                                         SigningAlgorithm = "RS256"
                                     }
                                 });
            }



            //* create builtin roles
            static async Task CreateRoles(ManagementApiClient client, string apiId, ISecurityInfo securityInfo)
            {
                if (securityInfo.BuiltInRoles != default)
                {
                    foreach (var role in securityInfo.BuiltInRoles)
                    {
                        await CreateRoleOnServer(client, apiId, securityInfo, role);
                    }
                }
            }
        }

        public async Task UpdateApiRegistrationWithProvider(
            ISecurityInfo securityInfo,
            Func<string, ValueTask> writeLine)
        {
            {

                var client = await GetManagementApiClientCached();
                
                await writeLine(
                    $"Updating Auth0 Api for this service in environment {this.config.Environment.Value} with the latest permissions.");

                GetApiId(this.config, out string apiId);
                
                await UpdateApiPermissions(client, securityInfo, apiId, writeLine);

                await UpdateApiRoles(client, securityInfo, apiId, writeLine);
            }

            static async Task UpdateApiRoles(ManagementApiClient client, ISecurityInfo securityInfo, string apiId, Func<string, ValueTask> writeLine)
            {
                {
                    var environmentPartitionKey = EnvVars.EnvironmentPartitionKey;
                    var serverRoles = await GetAllRoles(client);
                    var localRoleDefinitions = securityInfo.BuiltInRoles;

                    //* remove roles
                    var serverRolesToRemove = serverRoles
                                              .Where(r => localRoleDefinitions.All(lr => lr.AsAuth0Name(environmentPartitionKey) != r.Name))
                                              .ToList();
                    if (serverRolesToRemove.Any())
                    {
                        await writeLine(
                            "Removing Roles:" + Environment.NewLine + serverRolesToRemove.Select(x => x.Name)
                                                                                         .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    var removeTasks = serverRolesToRemove.Select(async sr => await RemoveServerRole(client, sr)).ToList();
                    await Task.WhenAll(removeTasks);

                    //* add roles
                    var localRolesToAdd = localRoleDefinitions.Where(lr => serverRoles.All(sr => sr.Name != lr.AsAuth0Name(environmentPartitionKey)))
                                                              .ToList();
                    if (localRolesToAdd.Any())
                    {
                        await writeLine(
                            "Adding Roles:" + Environment.NewLine + localRolesToAdd.Select(x => x.Value)
                                                                                   .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    var addTasks = localRolesToAdd.Select(async lr => await CreateRoleOnServer(client, apiId, securityInfo, lr)).ToList();
                    await Task.WhenAll(addTasks);

                    //* update permissions on existing roles
                    var serverCopyOfRolesThatAreStillRelevant = serverRoles
                                                                .Where(
                                                                    sr => localRoleDefinitions.Exists(
                                                                        lr => lr.AsAuth0Name(environmentPartitionKey) == sr.Name))
                                                                .ToList();
                    if (serverCopyOfRolesThatAreStillRelevant.Any())
                    {
                        await writeLine("Updating Existing Role Permissions....");
                    }

                    var updateTasks = serverCopyOfRolesThatAreStillRelevant.Select(
                        async sr =>
                            {
                            var matchingLocalRole = localRoleDefinitions.Single(x => x.AsAuth0Name(EnvVars.EnvironmentPartitionKey) == sr.Name);
                            await UpdateRoleApiPermissions(client, apiId, writeLine, matchingLocalRole, sr);
                            });
                    await Task.WhenAll(updateTasks);
                }

                static async Task UpdateRoleApiPermissions(
                    ManagementApiClient client,
                    string apiId,
                    Func<string, ValueTask> writeLine,
                    Interfaces.Role matchingLocalCopy,
                    global::Auth0.ManagementApi.Models.Role serverRoleThatNeedsItsApiPermissionsChecked)
                {
                    var serverSidePermissionsForTheRole =
                        await GetAllServerApiPermissionsForARole(client, serverRoleThatNeedsItsApiPermissionsChecked);
                    var localPermissions = matchingLocalCopy.ApiPermissions;

                    //* remove permissions
                    var permissionsToRemove = serverSidePermissionsForTheRole.Where(
                        sp => localPermissions.All(localPermission => localPermission.AsAuth0Claim(EnvVars.EnvironmentPartitionKey) != sp.Name)).ToList();
                    if (permissionsToRemove.Any())
                    {
                        await writeLine(
                            $"Removing Permissions To {matchingLocalCopy.Value} Role:" + Environment.NewLine
                                                                                          + permissionsToRemove.Select(x => x.Name)
                                                                                              .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    var removeTasks = permissionsToRemove.Select(
                        async p => await client.Roles.RemovePermissionsAsync(
                                       serverRoleThatNeedsItsApiPermissionsChecked.Id,
                                       new AssignPermissionsRequest()
                                       {
                                           Permissions = new List<PermissionIdentity>()
                                           {
                                               new PermissionIdentity()
                                               {
                                                   Identifier = p.Identifier, //TODO check this equals the apiID
                                                   Name = p.Name
                                               }
                                           }
                                       }));
                    await Task.WhenAll(removeTasks);

                    //* add permissions
                    var permissionsToAdd = localPermissions.Where(
                                                               lp => serverSidePermissionsForTheRole.All(
                                                                   sp => sp.Name != lp.AsAuth0Claim(EnvVars.EnvironmentPartitionKey)))
                                                           .ToList();
                    var addTasks = permissionsToAdd.Select(
                        async p => await client.Roles.AssignPermissionsAsync(
                                       serverRoleThatNeedsItsApiPermissionsChecked.Id,
                                       new AssignPermissionsRequest()
                                       {
                                           Permissions = new List<PermissionIdentity>()
                                           {
                                               new PermissionIdentity()
                                               {
                                                   Identifier = apiId,
                                                   Name = p.AsAuth0Claim(EnvVars.EnvironmentPartitionKey)
                                               }
                                           }
                                       }));
                    if (permissionsToAdd.Any())
                    {
                        await writeLine(
                            $"Adding Permissions To {matchingLocalCopy.Value} Role:" + Environment.NewLine + permissionsToAdd.Select(x => x.Value)
                                .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    await Task.WhenAll(addTasks);
                }

                static async Task RemoveServerRole(ManagementApiClient client, global::Auth0.ManagementApi.Models.Role role)
                {
                    await client.Roles.DeleteAsync(role.Id);
                }

                static async Task<List<Permission>> GetAllServerApiPermissionsForARole(
                    ManagementApiClient client,
                    global::Auth0.ManagementApi.Models.Role role)
                {
                    var allPermissions = await RetrieveAllPermissionsOnePageAtATime(client, role.Id, 0, new List<Permission>());
                    return allPermissions;

                    static async Task<List<Permission>> RetrieveAllPermissionsOnePageAtATime(
                        ManagementApiClient client,
                        string role,
                        int pageNo,
                        List<Permission> permissions)
                    {
                        var page = await client.Roles.GetPermissionsAsync(role, new PaginationInfo(pageNo, 50, true));

                        permissions.AddRange(page.ToList());

                        var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                        return thereAreMoreItems switch
                        {
                            true => await RetrieveAllPermissionsOnePageAtATime(client, role, ++pageNo, permissions),
                            _ => permissions
                        };
                    }
                }

                static async Task<List<global::Auth0.ManagementApi.Models.Role>> GetAllRoles(ManagementApiClient client)
                {
                    var allRoles = await RetrieveAllRolesOnePageAtATime(client, 0, new List<global::Auth0.ManagementApi.Models.Role>());
                    var filteredByTheCurrentPartitionKey = EnvVars.EnvironmentPartitionKey switch
                    {
                        var x when string.IsNullOrEmpty(x) => allRoles.Where(x => x.Name.StartsWith("builtin")).ToList(),
                        _ => allRoles.Where(x => x.Name.StartsWith($"{EnvVars.EnvironmentPartitionKey}::")).ToList() //* DEV environment
                    };

                    return filteredByTheCurrentPartitionKey;

                    static async Task<List<global::Auth0.ManagementApi.Models.Role>> RetrieveAllRolesOnePageAtATime(
                        ManagementApiClient client,
                        int pageNo,
                        List<global::Auth0.ManagementApi.Models.Role> roles)
                    {
                        var page = await client.Roles.GetAllAsync(new GetRolesRequest(), new PaginationInfo(pageNo, 50, true));

                        roles.AddRange(page.ToList());

                        var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                        return thereAreMoreItems switch
                        {
                            true => await RetrieveAllRolesOnePageAtATime(client, ++pageNo, roles),
                            _ => roles
                        };
                    }
                }
            }

            static async Task UpdateApiPermissions(
                ManagementApiClient client,
                ISecurityInfo securityInfo,
                string apiId,
                Func<string, ValueTask> writeLine)
            {
                {
                    var allScopes = await GetSetOfLatestPermissionSchemaIncludingThoseFromOtherPartitions();

                    var r = new ResourceServerUpdateRequest
                    {
                        Scopes = allScopes,
                        TokenDialect = TokenDialect.AccessTokenAuthZ //couldn't be 100% this wasn't overwritten so set it here as well as create
                    };

                    await client.ResourceServers.UpdateAsync(apiId, r);
                }

                async Task<List<ResourceServerScope>> GetSetOfLatestPermissionSchemaIncludingThoseFromOtherPartitions()
                {
                    /* seems there is not way just to update a partial set, you have to update the complete set for the whole API,
                    this means we have to calculate that and update only those for our environment partition. Thankfully as long as the
                    key isn't changing this doesn't wipe out existing assignments */

                    var resourceServer = await client.ResourceServers.GetAsync(apiId);
                    var onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions = resourceServer.Scopes;

                    SeparateOnlinePermissionsIntoGroups(
                        onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions,
                        out var onlineApiPermissionsFromOurPartition,
                        out var onlinePermissionsFromOtherPartitions);

                    var currentApiPermissionsInOurPartition = GetLocalApiPermissions(securityInfo);
                    var updatedApiPermissionsToBeSavedOnline =
                        currentApiPermissionsInOurPartition.Union(onlinePermissionsFromOtherPartitions).ToList();

                    var onlineApiPermissionsBeingRemoved = onlineApiPermissionsFromOurPartition
                                                           .Where(x => !currentApiPermissionsInOurPartition.Exists(y => y.Value == x.Value))
                                                           .ToList();
                    if (onlineApiPermissionsBeingRemoved.Any())
                    {
                        await writeLine(
                            "Removing Permissions:" + Environment.NewLine + onlineApiPermissionsBeingRemoved.Select(x => x.Value)
                                .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    var newApiPermissionsBeingAddedOnline = currentApiPermissionsInOurPartition.Where(
                                                                                                   x => !onlineApiPermissionsFromOurPartition.Exists(
                                                                                                       y => y.Value == x.Value))
                                                                                               .ToList();
                    if (newApiPermissionsBeingAddedOnline.Any())
                    {
                        await writeLine(
                            "Adding Permissions:" + Environment.NewLine + newApiPermissionsBeingAddedOnline.Select(x => x.Value)
                                .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    if (!newApiPermissionsBeingAddedOnline.Any() && !onlineApiPermissionsBeingRemoved.Any())
                    {
                        await writeLine("No Permissions Changes.");
                    }

                    return updatedApiPermissionsToBeSavedOnline;

                    static void SeparateOnlinePermissionsIntoGroups(
                        List<ResourceServerScope> onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions,
                        out List<ResourceServerScope> onlinePermissionsFromOurPartition,
                        out List<ResourceServerScope> onlinePermissionsFromOtherPartitions)
                    {
                        if (!string.IsNullOrEmpty(EnvVars.EnvironmentPartitionKey))
                        {
                            onlinePermissionsFromOtherPartitions = onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions
                                                                   .Where(x => !x.Value.StartsWith(EnvVars.EnvironmentPartitionKey))
                                                                   .ToList();
                            onlinePermissionsFromOurPartition = onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions
                                                                .Where(x => x.Value.StartsWith(EnvVars.EnvironmentPartitionKey))
                                                                .ToList();
                        }
                        else
                        {
                            onlinePermissionsFromOtherPartitions =
                                new List<ResourceServerScope>(); //* we are not in DEV environment so there won't be anything
                            onlinePermissionsFromOurPartition = onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions;
                        }
                    }
                }
            }
        }

        async Task<ManagementApiClient> GetManagementApiClientCached()
        {
            var mgmtToken = await GetManagementApiTokenViaHttp(this.config);

            if (managementApiClientCache == default || DateTime.UtcNow.Subtract(managementApiClientCache.expires).TotalHours > 23.0)
            {
                managementApiClientCache = (new ManagementApiClient(mgmtToken, new Uri($"https://{this.config.Auth0TenantDomain}/api/v2")),
                                               DateTime.UtcNow);
            }

            return managementApiClientCache.apiClient;

            //* admin token for making calls to the Auth0 Mgmt API
            static async Task<string> GetManagementApiTokenViaHttp(ApplicationConfig applicationConfig)
            {
                {
                    string adminToken = null;
                    await GetApiAccessToken(applicationConfig, v => adminToken = v, $"https://{applicationConfig.Auth0TenantDomain}/api/v2/");

                    return adminToken;
                }
            }
        }

        public class AppMetaData : IHaveRoles
        {
            public List<RoleInstance> Roles { get; set; } = new List<RoleInstance>();
        }


    }
}