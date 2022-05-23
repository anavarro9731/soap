namespace Soap.Auth0
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Newtonsoft.Json;
    using RestSharp;
    using Soap.Config;

    public static partial class Auth0Functions
    {
        public static class Tokens
        {
            private static RestClient restClient;
            
            public static async Task GetEnterpriseAdminM2MTokenForThisApi(ApplicationConfig applicationConfig, Action<string> setToken)
            {
                string adminM2MToken = null;
                await GetApiAccessToken(
                    applicationConfig,
                    v => adminM2MToken = v,
                    applicationConfig.FunctionAppHostUrlWithTrailingSlash);

                setToken(adminM2MToken);
            }

            public static async Task GetManagementApiToken(ApplicationConfig applicationConfig, Action<string> setToken)
            {
                string adminToken = null;
                await GetApiAccessToken(
                    applicationConfig,
                    v => adminToken = v,
                    $"https://{applicationConfig.Auth0TenantDomain}/api/v2/");

                setToken(adminToken);
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
