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
    
    public static partial class Auth0Functions
    {
        private static OpenIdConnectConfiguration cache_openIdConnectConfiguration;

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
                await writeLine(
                    $"Updating Auth0 Api for this service in environment {applicationConfig.Environment.Value} with the latest permissions.");

                var allScopes = await MergeScopesWithThoseOnline();

                var r = new ResourceServerUpdateRequest
                {
                    Scopes = allScopes,
                    TokenDialect =
                        TokenDialect.AccessTokenAuthZ //couldn't be 100% this wasnt overwritten so set it here as well as create
                };

                await client.ResourceServers.UpdateAsync(apiId, r);

                async Task<List<ResourceServerScope>> MergeScopesWithThoseOnline()
                {
                    var resourceServer = await client.ResourceServers.GetAsync(apiId);
                    var existingPermissionsWhichMightIncludeOtherDevelopersPermissionsAlreadySavedOnline = resourceServer.Scopes;

                    List<ResourceServerScope> existingPermissionsAlreadySavedOnlineThatWeDidNotCreate;
                    List<ResourceServerScope> existingPermissionsAlreadySavedOnlineThatWeDidCreate;
                    if (!string.IsNullOrEmpty(EnvVars.EnvironmentPartitionKey))
                    {
                        existingPermissionsAlreadySavedOnlineThatWeDidNotCreate =
                            existingPermissionsWhichMightIncludeOtherDevelopersPermissionsAlreadySavedOnline
                                .Where(x => !x.Value.StartsWith(EnvVars.EnvironmentPartitionKey))
                                .ToList();
                        existingPermissionsAlreadySavedOnlineThatWeDidCreate = 
                            existingPermissionsWhichMightIncludeOtherDevelopersPermissionsAlreadySavedOnline
                               .Where(x => x.Value.StartsWith(EnvVars.EnvironmentPartitionKey))
                               .ToList();
                    }
                    else
                    {
                        existingPermissionsAlreadySavedOnlineThatWeDidNotCreate = new List<ResourceServerScope>();
                        existingPermissionsAlreadySavedOnlineThatWeDidCreate =
                            existingPermissionsWhichMightIncludeOtherDevelopersPermissionsAlreadySavedOnline;
                    }
                    
                    var ourCurrentPermissions = GetPermissionsFromMessagesBasedOnOurParitionKey(securityInfo);
                    var finalUpdatedPermissions = ourCurrentPermissions.Union(existingPermissionsAlreadySavedOnlineThatWeDidNotCreate).ToList();
                    
                    var obsoletePermissions = existingPermissionsAlreadySavedOnlineThatWeDidCreate.Where(x => !ourCurrentPermissions.Exists(y => y.Value == x.Value)).ToList();
                    if (obsoletePermissions.Any())
                    {
                        await writeLine(
                            "Removing Permissions:" + Environment.NewLine + obsoletePermissions.Select(x => x.Value)
                                .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    var newPermissions = ourCurrentPermissions.Where(x => !existingPermissionsAlreadySavedOnlineThatWeDidCreate.Exists(y => y.Value == x.Value)).ToList();
                    if (newPermissions.Any())
                    {
                        await writeLine(
                            "Adding Permissions:" + Environment.NewLine + newPermissions.Select(x => x.Value)
                                .Aggregate((x, y) => $"{x}{Environment.NewLine}{y}"));
                    }

                    return finalUpdatedPermissions;
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
                await writeLine(
                    $"Auth0 Api for this service in environment {applicationConfig.Environment.Value} does not exist. Creating it now.");

                //* register api 
                var resourceServerScopes = GetPermissionsFromMessagesBasedOnOurParitionKey(securityInfo);
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

                //* give enterprise admin access to for inter-service calls
                await client.ClientGrants.CreateAsync(
                    new ClientGrantCreateRequest
                    {
                        Audience = apiId,
                        Scope = resourceServerScopes.Select(x => x.Value).ToList(),
                        ClientId = applicationConfig.Auth0EnterpriseAdminClientId //* enterprise admin clientid
                    });

                //* give frontend access by creating frontend app
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

            static List<ResourceServerScope> GetPermissionsFromMessagesBasedOnOurParitionKey(ISecurityInfo securityInfo)
            {
                var scopes = securityInfo.PermissionGroups.Select(
                                             x => new ResourceServerScope
                                             {
                                                 Description = x.Description ?? x.Name,
                                                 Value = x.AsClaim(EnvVars.EnvironmentPartitionKey)
                                             })
                                         .ToList();

                return scopes;
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

                    ExtractPermissionsFromClaims(principal, out var apiPermissionGroupsAsStrings, out var dbPermissionsAsStrings);

                    FilterOutBadPermissions(apiPermissionGroupsAsStrings, out var cleanPermissionGroups);

                    GetApiPermissionGroups(cleanPermissionGroups, securityInfo, out var permissionGroups);

                    GetDbPermissionsList(dbPermissionsAsStrings, out var dbPermissions);

                    GetApiPermissionsList(permissionGroups, out var permissions);

                    CreateIdentityPermissions(permissions, dbPermissions, out var identityPermissions);

                    return identityPermissions;

                    static void CreateIdentityPermissions(
                        List<string> apiPermissions,
                        List<DatabasePermissionInstance> dbPermissions,
                        out IdentityPermissions permissions)
                    {
                        permissions = new IdentityPermissions
                        {
                            ApiPermissions = apiPermissions,
                            DatabasePermissions = dbPermissions
                        };
                    }

                    static void GetApiPermissionsList(List<ApiPermissionGroup> permissionGroups, out List<string> permissions)
                    {
                        permissions = permissionGroups.SelectMany(x => x.ApiPermissions).ToList();
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

            static void FilterOutBadPermissions(string[] permissionsArray, out List<string> cleanPermissions)
            {
                /* if there are bad permissions data in auth0, we dont want to disable all users from logging in,
                we will just ignore those permissions and log the problem */
                var permissionRegex =
                    "^(" + DbPermissionsRegex() + "):[a-z0-9]+:[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}|(execute:[a-z0-9]+:[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12})$";
                cleanPermissions = permissionsArray.Select(x => x.ToLower().Trim())
                                                   .Where(x => Regex.IsMatch(x, permissionRegex))
                                                   .ToList();
            }

            static string DbPermissionsRegex() => $"{DatabasePermissions.READ.PermissionName.ToLower()}|{DatabasePermissions.CREATE.PermissionName.ToLower()}|{DatabasePermissions.UPDATE.PermissionName.ToLower()}|{DatabasePermissions.DELETE.PermissionName.ToLower()}";
            
            static void ExtractPermissionsFromClaims(
                ClaimsPrincipal principal,
                out string[] apiPermissionGroupsArray,
                out string[] dbPermissionsArray)
            {
                var permissionGroupsAsStrings =
                    principal.Claims.Where(x => x.Type == "permissions").Select(x => x.Value).ToArray();
                
                apiPermissionGroupsArray = permissionGroupsAsStrings.Where(x => x.Contains("execute"))
                                                                    .Select( /* replace environment specific key if it exists */
                                                                        x => x.Replace(x.SubstringBefore("::"), string.Empty)).ToArray();
                
                dbPermissionsArray = permissionGroupsAsStrings.Where(x => !x.Contains("execute"))
                            .Select( /* replace environment specific key if it exists */
                                x => x.Replace(x.SubstringBefore("::"), string.Empty)).ToArray();
                
            }

            static void GetApiPermissionGroups(
                List<string> permissionGroupsAsStrings,
                ISecurityInfo securityInfo,
                out List<ApiPermissionGroup> permissionGroups)
            {
                //* api permission format, execute:pingpong:134446A7-DA29-412F-AA1B-5FF9D1D0086C

                var permissionGroupIdsAsStrings = permissionGroupsAsStrings.Where(x => x.StartsWith("execute"))
                                                                           .Select(x => x.SubstringAfterLast(':'));
                permissionGroups = securityInfo.PermissionGroups
                                               .Where(pg => permissionGroupIdsAsStrings.Contains(pg.Id.ToString().ToLower()))
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
            var client = new ManagementApiClient(mgmtToken, new Uri($"https://{applicationConfig.Auth0TenantDomain}/api/v2"));
            setClient(client);
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
