namespace Soap.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using global::Auth0.ManagementApi.Paging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;
    using System.Text.RegularExpressions;
    using CircuitBoard;

    public static partial class Auth0Functions
    {
        private static OpenIdConnectConfiguration cache_openIdConnectConfiguration;

        private static (ManagementApiClient apiClient, DateTime expires) managementApiClient;

        public static async Task CheckAuth0Setup(
            ISecurityInfo securityInfo,
            ApplicationConfig applicationConfig,
            Func<string, ValueTask> writeLine)
        {
            
            {
                string managementApiToken = null;
                ManagementApiClient client = null;

                if (ConfigIsEnabledForAuth0Integration(applicationConfig))
                {
                    await Tokens.GetManagementApiToken(applicationConfig, v => managementApiToken = v);

                    GetManagementApiClient(managementApiToken, applicationConfig, v => client = v);

                    GetApiName(applicationConfig, out var apiName);

                    GetApiId(applicationConfig, out var apiId);

                    if (await ApiIsRegisteredWithAuth0(client, apiId))
                    {
                        await UpdateAuth0Registration(client, apiId, securityInfo, writeLine, applicationConfig);
                    }
                    else
                    {
                        await RegisterApiWithAuth0(client, apiId, apiName, securityInfo, writeLine, applicationConfig);
                    }
                }

                static bool ConfigIsEnabledForAuth0Integration(ApplicationConfig applicationConfig) =>
                    applicationConfig.AuthEnabled;
            }

            static void GetApiId(ApplicationConfig applicationConfig, out string apiId)
            {
                apiId = applicationConfig.FunctionAppHostUrlWithTrailingSlash;
            }

            static async Task<bool> ApiIsRegisteredWithAuth0(ManagementApiClient client, string apiId)
            {
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

            

            static async Task UpdateAuth0Registration(
                ManagementApiClient client,
                string apiId,
                ISecurityInfo securityInfo,
                Func<string, ValueTask> writeLine,
                ApplicationConfig applicationConfig)
            {
                {
                    await writeLine(
                        $"Updating Auth0 Api for this service in environment {applicationConfig.Environment.Value} with the latest permissions.");

                    
                    await UpdateApiPermissions(client, securityInfo, apiId, writeLine);
                    
                    await UpdateApiRoles(client, securityInfo, apiId, writeLine);
                }


                static async Task UpdateApiRoles(ManagementApiClient client, ISecurityInfo securityInfo, string apiId, Func<string, ValueTask> writeLine)
                {
                    
                    var environmentPartitionKey = EnvVars.EnvironmentPartitionKey;
                    {
                        var serverRoles = await GetAllRoles(client);
                        
                        var localRoleDefinitions = securityInfo.BuiltInRoles;

                        //* remove roles
                        var serverRolesToRemove = serverRoles.Where(r => localRoleDefinitions.All(lr => lr.AsAuth0Name(environmentPartitionKey) != r.Name)).ToList();
                        if (serverRolesToRemove.Any())
                        {
                            await writeLine(
                                "Removing Roles:" + Environment.NewLine + serverRolesToRemove.Select(x => x.Name)
                                                                                             .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                        }
                        var removeTasks = serverRolesToRemove.Select(async sr => await RemoveServerRole(client, sr)).ToList();
                        await Task.WhenAll(removeTasks);
                        
                        //* add roles
                        var localRolesToAdd = localRoleDefinitions.Where(lr =>
                            serverRoles.All(sr => sr.Name != lr.AsAuth0Name(environmentPartitionKey))).ToList();
                        if (localRolesToAdd.Any())
                        {
                            await writeLine(
                                "Adding Roles:" + Environment.NewLine + localRolesToAdd.Select(x => x.Id.Value)
                                                                                       .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                        }
                        var addTasks = localRolesToAdd.Select(async lr => await CreateRoleOnServer(client,apiId, securityInfo, lr)).ToList();
                        await Task.WhenAll(addTasks);
                        
                        //* update permissions on existing roles
                        var serverCopyOfRolesThatAreStillRelevant = serverRoles.Where(sr => localRoleDefinitions.Exists(lr => lr.AsAuth0Name(environmentPartitionKey) == sr.Name)).ToList();
                        if (serverCopyOfRolesThatAreStillRelevant.Any())
                        {
                            await writeLine("Updating Existing Role Permissions....");
                        }
                        var updateTasks = serverCopyOfRolesThatAreStillRelevant.Select(async sr =>
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
                        global::Soap.Interfaces.Role matchingLocalCopy,
                        global::Auth0.ManagementApi.Models.Role serverRoleThatNeedsItsApiPermissionsChecked)
                    {
                        var serverSidePermissionsForTheRole = await GetAllServerApiPermissionsForARole(client, serverRoleThatNeedsItsApiPermissionsChecked);
                        var localPermissions = matchingLocalCopy.ApiPermissions;
                        
                        //* remove permissions
                        var permissionsToRemove = serverSidePermissionsForTheRole.Where(sp => localPermissions.All(localPermission => AsAuth0Claim(localPermission, EnvVars.EnvironmentPartitionKey) != sp.Name));
                        if (permissionsToRemove.Any())
                        {
                            await writeLine(
                                $"Removing Permissions To {matchingLocalCopy.Id.Value} Role:" + Environment.NewLine + permissionsToRemove.Select(x => x.Name)
                                    .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                        }
                        var removeTasks = permissionsToRemove.Select(async p => await client.Roles.RemovePermissionsAsync(serverRoleThatNeedsItsApiPermissionsChecked.Id, new AssignPermissionsRequest()
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
                        var permissionsToAdd = localPermissions
                            .Where(
                                lp => serverSidePermissionsForTheRole.All(
                                    sp => sp.Name != AsAuth0Claim(lp, EnvVars.EnvironmentPartitionKey))).ToList();
                        var addTasks = permissionsToAdd
                                       .Select(async p => await client.Roles.AssignPermissionsAsync(
                                                              serverRoleThatNeedsItsApiPermissionsChecked.Id,
                                                              new AssignPermissionsRequest()
                                                              {
                                                                  Permissions = new List<PermissionIdentity>()
                                                                  {
                                                                    new PermissionIdentity()
                                                                    {
                                                                        Identifier = apiId,
                                                                        Name = AsAuth0Claim(p, EnvVars.EnvironmentPartitionKey)
                                                                    }
                                                                  }
                                                              }));
                        if (permissionsToAdd.Any())
                        {
                            await writeLine(
                                $"Adding Permissions To {matchingLocalCopy.Id.Value} Role:" + Environment.NewLine + permissionsToAdd.Select(x => x.Value)
                                    .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                        }
                        await Task.WhenAll(addTasks);

                        static string AsAuth0Claim(Enumeration apiPermission, string environmentPartitionKey)
                        {
                            return (!string.IsNullOrEmpty(environmentPartitionKey) ? environmentPartitionKey + "::" : string.Empty) 
                                   + Regex.Replace(apiPermission.Key.ToLower(), "[^a-z0-9./-]", string.Empty);
                        }
                    }
                    
                    static async Task RemoveServerRole(ManagementApiClient client, global::Auth0.ManagementApi.Models.Role role)
                    {
                        await client.Roles.DeleteAsync(role.Id);
                    }

                    static async Task<List<Permission>> GetAllServerApiPermissionsForARole(
                        ManagementApiClient client,
                        global::Auth0.ManagementApi.Models.Role role)
                    {
                        var allPermissions = await RetrieveAllPermissionsOnePageAtATime(client, role.Id, 0, new List<global::Auth0.ManagementApi.Models.Permission>());
                        return allPermissions;
                        
                        static async Task<List<global::Auth0.ManagementApi.Models.Permission>> RetrieveAllPermissionsOnePageAtATime(
                            ManagementApiClient client,
                            string roleId,
                            int pageNo,
                            List<global::Auth0.ManagementApi.Models.Permission> permissions)
                        {
                            var page = await client.Roles.GetPermissionsAsync(
                                           roleId,                   
                                           new PaginationInfo(pageNo, 50, true));

                            permissions.AddRange(page.ToList());

                            var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                            return thereAreMoreItems switch
                            {
                                true => await RetrieveAllPermissionsOnePageAtATime(client, roleId, ++pageNo, permissions),
                                _ => permissions
                            };
                        }
                    }
                    
                    static  async Task<List<global::Auth0.ManagementApi.Models.Role>> GetAllRoles(ManagementApiClient client)
                    {
                        var allRoles = await RetrieveAllRolesOnePageAtATime(client, 0, new List<global::Auth0.ManagementApi.Models.Role>());
                        var filteredByTheCurrentPartitionKey = EnvVars.EnvironmentPartitionKey switch
                        { 
                            var x when string.IsNullOrEmpty(x) => allRoles.Where(x => x.Name.StartsWith("builtin")).ToList(),  
                            _ => allRoles.Where(x => x.Name.StartsWith($"{EnvVars.EnvironmentPartitionKey}::")).ToList()  //* DEV environment
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

                static async Task UpdateApiPermissions(ManagementApiClient client, ISecurityInfo securityInfo, string apiId, Func<string, ValueTask> writeLine)
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
                        var updatedApiPermissionsToBeSavedOnline = currentApiPermissionsInOurPartition.Union(onlinePermissionsFromOtherPartitions).ToList();

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
                                onlinePermissionsFromOtherPartitions = new List<ResourceServerScope>();  //* we are not in DEV environment so there won't be anything
                                onlinePermissionsFromOurPartition = onlinePermissionsWhichMightIncludeThoseFromOtherEnvironmentPartitions;
                            }
                        }
                    }
                }
            }

            static async Task RegisterApiWithAuth0(
                ManagementApiClient client,
                string apiId,
                string apiName,
                ISecurityInfo securityInfo,
                Func<string, ValueTask> writeLine,
                ApplicationConfig applicationConfig)
            {
                {
                    var resourceServerScopes = GetLocalApiPermissions(securityInfo);
                    
                    await writeLine(
                        $"Auth0 Api for this service in environment {applicationConfig.Environment.Value} does not exist. Creating it now.");

                    await CreateApiWithPermissions(resourceServerScopes);

                    await GrantMachineTokenAccessToApi(resourceServerScopes);

                    await CreateFrontEnd();

                    await CreateRoles();
                }

                //* register api 
                async Task CreateApiWithPermissions(List<ResourceServerScope> resourceServerScopes)
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
                 async Task GrantMachineTokenAccessToApi(List<ResourceServerScope> resourceServerScopes)
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
                async Task CreateFrontEnd()
                {
                    var result = await client.Clients.CreateAsync(
                                     new ClientCreateRequest
                                     {
                                         Name = GetAppName(apiName),
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
                async Task CreateRoles()
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

            static List<ResourceServerScope> GetLocalApiPermissions(ISecurityInfo securityInfo)
            {
                var scopes = securityInfo.ApiPermissions.Select(
                                             x => new ResourceServerScope
                                             {
                                                 Description = x.Description ?? x.Id.Value,
                                                 Value = x.AsAuth0Claim(EnvVars.EnvironmentPartitionKey)
                                             })
                                         .ToList();

                return scopes;
            }
        }

        private static async Task CreateRoleOnServer(ManagementApiClient client, string apiId, ISecurityInfo securityInfo, global::Soap.Interfaces.Role role)
        {
            var newRole = await client.Roles.CreateAsync(
                              new RoleCreateRequest()
                              {
                                  Description = role.Description ?? role.Id.Value,
                                  Name = role.AsAuth0Name(EnvVars.EnvironmentPartitionKey)
                              });

            if (role.ApiPermissions.Any())
            {
                await client.Roles.AssignPermissionsAsync(
                    newRole.Id,
                    new AssignPermissionsRequest()
                    {
                        Permissions = role.ApiPermissions.Select(
                                              apiPermissionEnum => new PermissionIdentity()
                                              {
                                                  Identifier = apiId,
                                                  Name = securityInfo.ApiPermissionFromEnum(apiPermissionEnum)
                                                                     .AsAuth0Claim(EnvVars.EnvironmentPartitionKey)
                                              })
                                          .ToList()
                    });
            }
        }

        public static async Task<IdentityPermissions> GetPermissionsFromAccessToken(
            ApplicationConfig applicationConfig,
            string bearerToken,
            ISecurityInfo securityInfo)
        {
            {
                var openIdConfig = await GetOpenIdConfig($"{applicationConfig.Auth0TenantDomain}");

                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidAudience = EnvVars.FunctionAppHostUrlWithTrailingSlash,
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

                    ExtractPermissionsFromClaims(principal, out var apiPermissionsAsStrings, out var dbPermissionsAsStrings);

                    FilterOutBadApiPermissions(apiPermissionsAsStrings, out var cleanedApiPermissions);

                    GetApiPermissions(cleanedApiPermissions, securityInfo, out var apiPermissions);

                    GetDbPermissionsList(dbPermissionsAsStrings, out var dbPermissions);

                    GetDeveloperPermissionsList(apiPermissions, out var developerPermissions);

                    CreateIdentityPermissions(developerPermissions, dbPermissions, out var identityPermissions);

                    return identityPermissions;

                    static void CreateIdentityPermissions(
                        List<string> developerPermissions,
                        List<DatabasePermissionInstance> dbPermissions,
                        out IdentityPermissions permissions)
                    {
                        permissions = new IdentityPermissions
                        {
                            ApiPermissions = developerPermissions,
                            DatabasePermissions = dbPermissions
                        };
                    }

                    static void GetDeveloperPermissionsList(List<ApiPermission> apiPermissions, out List<string> permissions)
                    {
                        permissions = apiPermissions.SelectMany(x => x.DeveloperPermissions).ToList();
                    }

                    static void GetDbPermissionsList(
                        string[] dbPermissionsAsStrings,
                        out List<DatabasePermissionInstance> dbPermissions)
                    {
                        dbPermissions = dbPermissionsAsStrings.Select(
                                                                  x =>
                                                                      {
                                                                      //permission format read:OptionalScopeObjectType:134446A7-DA29-412F-AA1B-5FF9D1D0086C
                                                                      var databasePermissionString =
                                                                          x.SubstringBefore(':').Trim().ToUpper();
                                                                      var databasePermissionScopeId =
                                                                          Guid.Parse(x.SubstringAfterLast(':').Trim());

                                                                      var databasePermission = databasePermissionString switch
                                                                      {
                                                                          nameof(DatabasePermissions.READ) => DatabasePermissions
                                                                              .READ,
                                                                          nameof(DatabasePermissions.CREATE) =>
                                                                              DatabasePermissions.CREATE,
                                                                          nameof(DatabasePermissions.UPDATE) =>
                                                                              DatabasePermissions.UPDATE,
                                                                          nameof(DatabasePermissions.DELETE) =>
                                                                              DatabasePermissions.DELETE,
                                                                          _ => throw new ApplicationException(
                                                                                   "Database permission type not valid")
                                                                      };

                                                                      return new DatabasePermissionInstance(
                                                                          databasePermission,
                                                                          new List<DatabaseScopeReference>
                                                                          {
                                                                              new DatabaseScopeReference(
                                                                                  databasePermissionScopeId)
                                                                          });
                                                                      })
                                                              .ToList();
                    }
                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new ApplicationException("The access token is expired.", ex);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("The access token is invalid.", e);
                }
            }

            static void FilterOutBadApiPermissions(string[] apiPermissionsArray, out List<string> cleanApiPermissions)
            {
                /* if there are bad permissions data in auth0, we dont want to disable all users from logging in,
                we will just ignore those permissions and log the problem */
                var permissionRegex =
                    "^(" + DbPermissionsRegex() + "):[a-z0-9]+:[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}|(execute:[a-z0-9]+:[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12})$";
                cleanApiPermissions = apiPermissionsArray.Select(x => x.ToLower().Trim())
                                                   .Where(x => Regex.IsMatch(x, permissionRegex))
                                                   .ToList();
            }

            static string DbPermissionsRegex() => $"{DatabasePermissions.READ.PermissionName.ToLower()}|{DatabasePermissions.CREATE.PermissionName.ToLower()}|{DatabasePermissions.UPDATE.PermissionName.ToLower()}|{DatabasePermissions.DELETE.PermissionName.ToLower()}";
            
            static void ExtractPermissionsFromClaims(
                ClaimsPrincipal principal,
                out string[] apiPermissionsArray,
                out string[] dbPermissionsArray)
            {
                var permissionGroupsAsStrings =
                    principal.Claims.Where(x => x.Type == "permissions").Select(x => x.Value).ToArray();

                permissionGroupsAsStrings = FilterToAppropriateEnvironmentPartition(permissionGroupsAsStrings);
                
                apiPermissionsArray = permissionGroupsAsStrings.Where(x => x.Contains("execute")).ToArray();
                
                dbPermissionsArray = permissionGroupsAsStrings.Where(x => !x.Contains("execute")).ToArray();
            }

            static string[] FilterToAppropriateEnvironmentPartition(string[] permissions)
            {
                var key = EnvVars.EnvironmentPartitionKey;

                var result = string.IsNullOrEmpty(key)
                           ? permissions.Where(p => !p.Contains("::"))
                           : permissions.Where(p => p.StartsWith(key));
                
                result = result.Select(RemoveEnvironmentPartitionKeyIfItExists);

                return result.ToArray();
                
                static string RemoveEnvironmentPartitionKeyIfItExists(string x)
                {
                    //* removes "johndoedeveloper::" prefix
                    return x.Replace(x.SubstringBefore("::"), string.Empty).Replace("::", string.Empty);
                }
            }
            
            static void GetApiPermissions(
                List<string> apiPermissionsAsStrings,
                ISecurityInfo securityInfo,
                out List<ApiPermission> apiPermissions)
            {
                //* api permission format, execute:pingpong:ping-pong

                var apiPermissionIdsAsStrings = apiPermissionsAsStrings.Where(x => x.StartsWith("execute"))
                                                                           .Select(x => x.SubstringAfterLast(':'));
                apiPermissions = securityInfo.ApiPermissions
                                               .Where(pg => apiPermissionIdsAsStrings.Contains(pg.Id.ToString().ToLower()))
                                               .ToList();
            }
        }

        public static async Task<string> GetUiApplicationClientId(ApplicationConfig applicationConfig, Assembly messagesAssembly)
        {
            {
                string mgmtToken = null;
                ManagementApiClient client = null;

                await Tokens.GetManagementApiToken(applicationConfig, v => mgmtToken = v);

                GetManagementApiClient(mgmtToken, applicationConfig, v => client = v);

                GetApiName(applicationConfig, out var apiName);

                var appName = GetAppName(apiName);

                var appClientId = await ProcessPageOfApiClientApplicationResults(client, 0, appName);

                return appClientId;
            }

            static async Task<string> ProcessPageOfApiClientApplicationResults(
                ManagementApiClient client,
                int pageNo,
                string appName)
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

        private static void GetApiName(ApplicationConfig applicationConfig, out string apiName)
        {
            apiName = applicationConfig.AppId;
        }

        private static string GetAppName(string apiName) => $"{apiName}.ui";

        private static void GetManagementApiClient(
            string mgmtToken,
            ApplicationConfig applicationConfig,
            Action<ManagementApiClient> setClient)
        {
            if (managementApiClient == default || DateTime.UtcNow.Subtract(managementApiClient.expires).TotalHours > 23.0)
            {
                managementApiClient = (new ManagementApiClient(mgmtToken, new Uri($"https://{applicationConfig.Auth0TenantDomain}/api/v2")), DateTime.UtcNow);   
            }

            setClient(managementApiClient.apiClient);
        }

        private static async Task<OpenIdConnectConfiguration> GetOpenIdConfig(string tenantDomain)
        {
            //* Get the public keys from the jwks endpoint   

            //* cache til static expires in azure functions, as its expensive
            cache_openIdConnectConfiguration ??= await GetOpenIdConfigInternal(tenantDomain);
            return cache_openIdConnectConfiguration;

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
    }
}
