namespace Soap.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using global::Auth0.ManagementApi.Paging;
    using Microsoft.CSharp.RuntimeBinder;
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

                    GetManagementApiClient(managementApiToken, v => client = v);

                    GetApiName(messagesAssembly, out var apiName);

                    GetApiId(out var apiId);

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

                void GetManagementApiClient(string mgmtToken, Action<ManagementApiClient> setClient)
                {
                    var client = new ManagementApiClient(mgmtToken, new Uri($"{applicationConfig.Auth0TenantDomain}/api/v2"));
                    setClient(client);
                }
            }

            static void GetApiName(Assembly messagesAssembly, out string apiName)
            {
                apiName = messagesAssembly.GetName().Name.ToLower().SubstringBefore(".messages");
            }

            static void GetApiId(out string apiId)
            {
                apiId = EnvVars.FunctionAppHostUrl;
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

            static void GetManagementApiToken(ApplicationConfig applicationConfig, Action<string> setMgmtToken)
            {
                var client = new RestClient($"{applicationConfig.Auth0TenantDomain}/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddParameter(
                    "application/json",
                    $"{{\"client_id\":\"{applicationConfig.Auth0EnterpriseAdminClientId}\",\"client_secret\":\"{applicationConfig.Auth0EnterpriseAdminClientSecret}\",\"audience\":\"{applicationConfig.Auth0TenantDomain}/api/v2/\",\"grant_type\":\"client_credentials\"}}",
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
                if (string.IsNullOrWhiteSpace(accessToken)) throw new ApplicationException("Could not retrieve Auth0 access token for management api, it was blank");
                
                setMgmtToken(accessToken);
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

                var resourceServerScopes = GetScopesFromMessages(securityInfo);
                var r = new ResourceServerCreateRequest
                {
                    Identifier = apiId,
                    Name = apiName,
                    Scopes = resourceServerScopes,
                    TokenDialect = TokenDialect.AccessTokenAuthZ,
                    AllowOfflineAccess = true,
                    SkipConsentForVerifiableFirstPartyClients = true
                };

                await client.ResourceServers.CreateAsync(r);
                
                //* give enterprise admin access to for inter-service calls
                await client.ClientGrants.CreateAsync(
                    new ClientGrantCreateRequest()
                    {
                        Audience = apiId,
                        Scope = resourceServerScopes.Select(x => x.Value).ToList(),
                        ClientId = applicationConfig.Auth0EnterpriseAdminClientId //* enterprise admin clientid
                    });
                
                //* give frontend access
                var result = await client.Clients.CreateAsync(
                    new ClientCreateRequest()
                    {
                        Name = $"{apiName}.ui",
                        Description = $"{apiName} front end",
                        ApplicationType = ClientApplicationType.Spa,
                        IsFirstParty = true,
                        Callbacks = new []{applicationConfig.CorsOrigin},
                        AllowedLogoutUrls = new []{applicationConfig.CorsOrigin}
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
    }
}
