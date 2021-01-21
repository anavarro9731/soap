namespace Soap.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using global::Auth0.ManagementApi.Paging;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using RestSharp;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public static class Auth0Functions
    {
        public static async Task CheckAuth0Setup(
            Assembly messagesAssembly,
            ISecurityInfo securityInfo,
            ApplicationConfig applicationConfig,
            Func<string, ValueTask> writeLine)
        {
            {
                string managementApiToken = null;
                ManagementApiClient client = null;

                if (ConfigIsEnabledForAuth0Integration(applicationConfig))
                {
                    GetManagementApiToken(applicationConfig, v => managementApiToken = v);

                    GetManagementApiClient(managementApiToken, applicationConfig, v => client = v);

                    GetApiName(messagesAssembly, out var apiName);

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
                    applicationConfig.Auth0Enabled;
            }

            static void GetApiId(ApplicationConfig applicationConfig, out string apiId)
            {
                apiId = applicationConfig.FunctionAppHostUrlWithTrailingSlash;
            }

            static async Task<bool> ApiIsRegisteredWithAuth0(ManagementApiClient client, string apiId)
            {
                var exists = await ProcessPage(client, 0, apiId);
                return exists;

                static async Task<bool> ProcessPage(ManagementApiClient client, int pageNo, string apiId)
                {
                    var page = await client.ResourceServers.GetAllAsync(new PaginationInfo(pageNo, 50, true));
                    if (page.Any(server => server.Identifier == apiId))
                    {
                        return true;
                    }

                    var thereAreMoreItems = page.Paging.Total == page.Paging.Limit;
                    return thereAreMoreItems switch
                    {
                        true => await ProcessPage(client, ++pageNo, apiId),
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

                var r = new ResourceServerUpdateRequest
                {
                    Scopes = GetScopesFromMessages(securityInfo)
                };

                await client.ResourceServers.UpdateAsync(apiId, r);
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
                var resourceServerScopes = GetScopesFromMessages(securityInfo);
                var r = new ResourceServerCreateRequest
                {
                    Identifier = apiId,
                    Name = apiName,
                    Scopes = resourceServerScopes,
                    TokenDialect = TokenDialect.AccessTokenAuthZ,
                    EnforcePolicies = true, //RBAC ENABLED?
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

            static List<ResourceServerScope> GetScopesFromMessages(ISecurityInfo securityInfo)
            {
                var scopes = securityInfo.PermissionGroups.Select(
                                             x => new ResourceServerScope
                                             {
                                                 Description = x.Description ?? x.Name,
                                                 Value = "execute:" + Regex.Replace(x.Name.ToLower(), "[^a-zA-z0-9.]", "")
                                             })
                                         .ToList();

                return scopes;
            }
        }

        public static async Task<string> GetUiApplicationClientId(ApplicationConfig applicationConfig, Assembly messagesAssembly)
        {
            {
                string mgmtToken = null;
                ManagementApiClient client = null;

                GetManagementApiToken(applicationConfig, v => mgmtToken = v);

                GetManagementApiClient(mgmtToken, applicationConfig, v => client = v);

                GetApiName(messagesAssembly, out var apiName);

                var appName = GetAppName(apiName);

                var appClientId = await ProcessPage(client, 0, appName);

                return appClientId;
            }

            static async Task<string> ProcessPage(ManagementApiClient client, int pageNo, string appName)
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
                    true => await ProcessPage(client, ++pageNo, appName),
                    _ => null
                };
            }
        }

        public static async Task AuthoriseCall(ApplicationConfig applicationConfig, string bearerToken)
        {
            {
                if (applicationConfig.Auth0Enabled) //TODO and message is authorisable
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

                    var handler = new JwtSecurityTokenHandler();
                    try
                    {
                        //* will validate formation and signature by default
                        var principal = handler.ValidateToken(bearerToken, validationParameters, out var validatedToken);

                        //* find the scope claims, get its contents and check you have the scope for this message
                        //Guard.Against(principal.Claims.Single(x => x.), "This access token is not valid for this message"); //TODO filter
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
            }

            // Get the public keys from the jwks endpoint      
            async Task<OpenIdConnectConfiguration> GetOpenIdConfig(string tenantDomain)
            {
                var openIdConfigurationEndpoint = $"https://{tenantDomain}/.well-known/openid-configuration";
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    openIdConfigurationEndpoint,
                    new OpenIdConnectConfigurationRetriever());
                var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
                return openIdConfig;
            }
        }

        private static void GetApiName(Assembly messagesAssembly, out string apiName)
        {
            apiName = messagesAssembly.GetName().Name.ToLower().SubstringBefore(".messages");
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

        private static void GetManagementApiToken(ApplicationConfig applicationConfig, Action<string> setMgmtToken)
        {
            var client = new RestClient($"https://{applicationConfig.Auth0TenantDomain}/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddParameter(
                "application/json",
                $"{{\"client_id\":\"{applicationConfig.Auth0EnterpriseAdminClientId}\",\"client_secret\":\"{applicationConfig.Auth0EnterpriseAdminClientSecret}\",\"audience\":\"https://{applicationConfig.Auth0TenantDomain}/api/v2/\",\"grant_type\":\"client_credentials\"}}",
                ParameterType.RequestBody);
            var response = client.Execute(request);
            var tokenJson = response.Content;
            dynamic tokenObject = JsonConvert.DeserializeObject(tokenJson);
            string accessToken = null;
            try
            {
                accessToken = tokenObject.access_token;
            }
            catch (RuntimeBinderException)
            {
                throw new ApplicationException(
                    "Response was invalid when attempting to obtain Auth0 Management Api access token");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ApplicationException("Could not retrieve Auth0 access token for management api, it was blank");
            }

            setMgmtToken(accessToken);
        }
    }
}
