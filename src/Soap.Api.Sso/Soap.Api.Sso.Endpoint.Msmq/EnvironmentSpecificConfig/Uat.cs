namespace Soap.Api.Sso.Endpoint.Msmq.EnvironmentSpecificConfig
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Api.Sso.Domain.Logic.Configuration;
    using Soap.If.Interfaces;
    using Soap.Pf.EndpointInfrastructure;

    public class Uat : IEnvironmentSpecificConfig
    {
        //for security we don't check these values into source control
        public IApplicationConfig Variables => JsonConvert.DeserializeObject<ApplicationConfiguration>(Environment.GetEnvironmentVariable("ApplicationConfig"));

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithProperty("Environment", nameof(Uat))
                .Enrich.WithProperty("Application", Variables.ApplicationName)
                .Enrich.WithExceptionDetails();

            var seqConfig = ((ApplicationConfiguration)Variables).SeqLoggingSettings;
            if (seqConfig != null)
            {
                loggerConfiguration.WriteTo.Seq(seqConfig.ServerUrl, apiKey: seqConfig.ApiKey);
            }

            SelfLog.Enable(msg => Trace.TraceError(msg)); //when seq connection fails azure trace logs will pick this up
        }
    }
}