namespace Palmtree.Sample.Api.Endpoint.Msmq.EnvironmentSpecificConfig
{
    using System;
    using System.Collections.Generic;
    using DataStore.Impl.DocumentDb.Config;
    using DataStore.Impl.SqlServer;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Palmtree.ApiPlatform.Endpoint.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.ThirdPartyClients.Mailgun;
    using Palmtree.Sample.Api.Domain.Logic;

    public class Development : IEnvironmentSpecificConfig
    {
        public IApplicationConfig Variables => ApplicationConfiguration.Create(
            nameof(Development),
            ApiServerSettings.Create("http://localhost:5055", $"serviceapi@{Environment.MachineName}"),
            SqlServerDbSettings.Create("anavarro9731-sqlserver.database.windows.net", "serviceapi", "anavarro9731", "qtPn8aLGXcv3pVZs", "Aggregates"),
            FileStorageSettings.Create(
                "DefaultEndpointsProtocol=https;AccountName=anavarro9731serviceapi;AccountKey=qPP3MheZwotkD6bAbSuzTCUKX1B5rsa+L0GvDEbPZVgRgxZtr6Fl2nWcmoPvk6rb6Z0TXnjq7C4v967Wcm7MaQ==;BlobEndpoint=https://anavarro9731Palmtree.ApiPlatform.blob.core.windows.net/;",
                string.Empty),
            MailgunEmailSenderSettings.Create(
                "Mailgun Sandbox <postmaster@sandboxfa97c8c997f64d29a75c2453725b78e0.mailgun.org>",
                "key-101c1b392bb95000da55a349848aacd0",
                "sandboxfa97c8c997f64d29a75c2453725b78e0.mailgun.org",
                new List<string>
                {
                    "anavarro9731@gmail.com"
                }),
            1,
            "An error has occurred.",
            true,
            "Trace One API - MSMQ");

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", nameof(Development))
                                                           .Enrich.WithProperty("Application", "Palmtree.ApiPlatform.Endpoint.Rebus")
                                                           .Enrich.WithProperty("Transport", "Rebus")
                                                           .Enrich.WithExceptionDetails()
                                                           .WriteTo.ColoredConsole()
                                                           .WriteTo.Seq("http://13.81.4.220", apiKey: "1OSAbhW4o6ekOctXiwyk");

            SelfLog.Enable(Console.Error); //when seq connection fails write to console
        }
    }
}
