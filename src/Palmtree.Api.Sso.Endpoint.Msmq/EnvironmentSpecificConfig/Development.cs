namespace Palmtree.Api.Sso.Endpoint.Msmq.EnvironmentSpecificConfig
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Models;
    using Soap.ThirdPartyClients.Mailgun;

    public class Development : IEnvironmentSpecificConfig
    {
        public IApplicationConfig Variables => ApplicationConfiguration.Create(
            nameof(Development),
            ApiEndpointSettings.Create("http://localhost:5055", $"serviceapi@{Environment.MachineName}"),
            Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
            SqlServerDbSettings.Create(".", "soap", "sa", "SuperDuper", "Aggregates"),
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
            "PalmTree SSO - MSMQ",
            SeqLoggingConfig.Create("http://localhost:5341/"));

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