namespace Palmtree.Api.Sso.Endpoint.Http.EnvironmentSpecificConfig
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using DataStore.Impl.SqlServer;
    using Destructurama;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.HttpEndpointBase;

    public class Development : IHttpEnvironmentSpecificConfiguration
    {
        public IApplicationConfig Variables => ApplicationConfiguration.Create(nameof(Development), 
            Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
            ApiEndpointSettings.Create("http://localhost:5055", $"serviceapi@{Environment.MachineName}"),
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
            "PalmTree SSO - HTTP"
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
                                                           .Destructure.UsingAttributes()
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