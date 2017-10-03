namespace Palmtree.Api.Sso.Endpoint.Http.EnvironmentSpecificConfig
{
    using System;
    using System.Collections.Generic;
    using DataStore.Impl.DocumentDb.Config;
    using DataStore.Impl.SqlServer;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Endpoint.Http.Infrastructure;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Models;
    using Soap.ThirdPartyClients.Mailgun;

    public class Development : IHttpEnvironmentSpecificConfiguration
    {
        public IApplicationConfig Variables => ApplicationConfiguration.Create(
            nameof(Development),
            ApiServerSettings.Create("http://localhost:5055", $"serviceapi@{Environment.MachineName}"),
            SqlServerDbSettings.Create("anavarro9731-sqlserver.database.windows.net", "serviceapi", "anavarro9731", "qtPn8aLGXcv3pVZs", "Aggregates"),
            FileStorageSettings.Create(
                "DefaultEndpointsProtocol=https;AccountName=anavarro9731serviceapi;AccountKey=vZ3Rp3+wUrbdNQxTDaf+Za4XoiUtXeQ5GxWQU+WphJ9SdZWohA9ceCnbIlyI+y6Fehq8PZc8340fuqZoV+2jDw==;BlobEndpoint=https://anavarro9731Soap.blob.core.windows.net/;",
                string.Empty),
            MailgunEmailSenderSettings.Create(
                "Mailgun Sandbox <postmaster@sandboxfa97c8c997f64d29a75c2453725b78e0.mailgun.org>",
                "key-101c1b392bb95000da55a349848aacd0",
                "sandboxfa97c8c997f64d29a75c2453725b78e0.mailgun.org",
                new List<string>
                {
                    "anavarro9731@gmail.com"
                }),
            0,
            "An error has occurred.",
            true,
            "Trace One API - HTTP");

        public void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder)
        {
            policyBuilder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowCredentials().Build();
        }

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", nameof(Development))
                                                           .Enrich.WithProperty("Application", Variables.ApplicationName)
                                                           .Enrich.WithExceptionDetails()
                                                           .WriteTo.ColoredConsole()
                                                           .WriteTo.Seq("http://13.81.4.220", apiKey: "1OSAbhW4o6ekOctXiwyk");

            SelfLog.Enable(Console.Error); //when seq connection fails write to console
        }
    }
}
