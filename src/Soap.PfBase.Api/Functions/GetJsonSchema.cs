namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soap.Config;
    using Soap.Idaam;

    public static partial class PlatformFunctions
    {
        public static async Task<IActionResult> GetJsonSchema(HttpRequest req, ILogger log, Assembly messagesAssembly)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var tasks = new List<Task>();

                dynamic result = null;
                
                tasks.Add(
                    Task.Run(
                        () =>
                            {
                            result = DiagnosticFunctions.GetSchema(appConfig, messagesAssembly).AsJson;
                            }));
                
                tasks.Add(SetAuth0Headers(appConfig));

                //* these both take a bit of time, and hinder startup; run them in parallel; GetJsonSchema will take longer
                await Task.WhenAll(tasks);
                
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                logger?.Fatal(e, $"Could not execute function {nameof(GetJsonSchema)}");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }

            async Task SetAuth0Headers(ApplicationConfig appConfig)
            {
                AddHeader(req, "Access-Control-Expose-Headers", "*");
                AddHeader(req, "Idaam-Enabled", appConfig.AuthLevel.ApiPermissionEnabled.ToString().ToLower());
                
                if (appConfig.AuthLevel.ApiPermissionEnabled)
                {
                    AddHeader(req, "Auth0-Tenant-Domain", appConfig.Auth0TenantDomain);
                    var idaamProvider = new IdaamProvider(appConfig);
                    var applicationClientId = await idaamProvider.GetUiApplicationClientId(messagesAssembly);
                    var apiId = idaamProvider.GetApiClient();
                    AddHeader(req, "Auth0-UI-Application-ClientId", applicationClientId);
                    AddHeader(req, "Auth0-UI-Api-ClientId", apiId);
                    AddHeader(req, "Auth0-Redirect-Uri", appConfig.CorsOrigin);
                }

                static void AddHeader(HttpRequest req, string headerName, string headerValue)
                {
                    req.HttpContext.Response.Headers.Add(headerName, headerValue);
                }
            }
        }
    }
}
