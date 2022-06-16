namespace Soap.Auth0
{
    using System;
    using System.Threading.Tasks;
<<<<<<< HEAD:src/Soap.Auth0/Auth0Functions.Internal.cs
    using global::Auth0.ManagementApi;
=======
    using CircuitBoard;
>>>>>>> origin/master:src/Soap.Auth0/Auth0Functions.Tokens.cs
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Newtonsoft.Json;
    using RestSharp;
    using Soap.Config;

    public static partial class Auth0Functions
    {
        public static class Internal
        {
            private static RestClient restClient;
            private static (ManagementApiClient apiClient, DateTime expires) managementApiClientCache;
            
            internal static async Task GetManagementApiClientCached(
                ApplicationConfig applicationConfig,
                Action<ManagementApiClient> set)
            {
                var mgmtToken = await Internal.GetManagementApiTokenViaHttp(applicationConfig);
            
                if (managementApiClientCache == default || DateTime.UtcNow.Subtract(managementApiClientCache.expires).TotalHours > 23.0)
                {
                    managementApiClientCache = (new ManagementApiClient(mgmtToken, new Uri($"https://{applicationConfig.Auth0TenantDomain}/api/v2")), DateTime.UtcNow);   
                }

                set(managementApiClientCache.apiClient);
            }
            
            /* admin token for making calls to our API
             depending on your Auth0plan, these can be limited in number you can acquire without incurring extra costs
             last check the free plan only supported 1K/mo this is why we use the internal servicetoken instead but im keeping the function here for reference */
            internal static async Task<string> GetEnterpriseAdminM2MTokenForThisApi(ApplicationConfig applicationConfig)
            {
                string adminM2MToken = null;
                await GetApiAccessToken(
                    applicationConfig,
                    v => adminM2MToken = v,
                    applicationConfig.FunctionAppHostUrlWithTrailingSlash);

                return adminM2MToken;
            }

            //* admin token for making calls to the Auth0 Mgmt API
            private static async Task<string> GetManagementApiTokenViaHttp(ApplicationConfig applicationConfig)
            {
                string adminToken = null;
                await GetApiAccessToken(
                    applicationConfig,
                    v => adminToken = v,
                    $"https://{applicationConfig.Auth0TenantDomain}/api/v2/");

                return adminToken;
            }
            
            private static async Task GetApiAccessToken(
                ApplicationConfig applicationConfig,
                Action<string> setApiAccessToken,
                string audience)
            {
                restClient ??= new RestClient($"https://{applicationConfig.Auth0TenantDomain}/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddParameter(
                    "application/json",
                    $"{{\"client_id\":\"{applicationConfig.Auth0EnterpriseAdminClientId}\",\"client_secret\":\"{applicationConfig.Auth0EnterpriseAdminClientSecret}\",\"audience\":\"{audience}\",\"grant_type\":\"client_credentials\"}}",
                    ParameterType.RequestBody);
                var response = await restClient.ExecuteAsync(request);
                var tokenJson = response.Content;
                dynamic tokenObject = JsonConvert.DeserializeObject(tokenJson);
                string accessToken = null;
                try
                {
                    accessToken = tokenObject.access_token;
                }
                catch (RuntimeBinderException)
                {
                    throw new CircuitException(
                        "Response was invalid when attempting to obtain Auth0 Management Api access token");
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new CircuitException("Could not retrieve Auth0 access token for management api, it was blank");
                }

                setApiAccessToken(accessToken);
            }
        }
    }
}
