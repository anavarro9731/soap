namespace Palmtree.Api.Sso.Endpoint.Http.EnvironmentSpecificConfig
{
    using System;
    using System.Collections.Generic;
    using DataStore.Impl.SqlServer;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Endpoint.Http.Infrastructure;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Models;
    using Soap.ThirdPartyClients.Mailgun;

    public class Development : IHttpEnvironmentSpecificConfiguration
    {
        public IApplicationConfig Variables => ApplicationConfiguration.Create(
            nameof(Development),
            ApiServerSettings.Create("http://localhost:5055", $"serviceapi@{Environment.MachineName}"),
            SqlServerDbSettings.Create(".", "soap", "sa", "SuperDuper", "Aggregates"),
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
            "PalmTree SSO - HTTP",
            SeqLoggingConfig.Create("http://localhost:5341/")
            );

        public void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder)
        {
            policyBuilder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowCredentials().Build();
        }

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", nameof(Development))
                                                           .Enrich.WithProperty("Application", Variables.ApplicationName)
                                                           .Enrich.WithExceptionDetails()
                                                           .WriteTo.ColoredConsole();

            var seqConfig = ((ApplicationConfiguration)Variables).SeqLoggingConfig;
            if (seqConfig != null)
            {
                loggerConfiguration.WriteTo.Seq(seqConfig.ServerUrl, apiKey: seqConfig.ApiKey);
            }

            SelfLog.Enable(Console.Error); //when seq connection fails write to console
        }
    }
}