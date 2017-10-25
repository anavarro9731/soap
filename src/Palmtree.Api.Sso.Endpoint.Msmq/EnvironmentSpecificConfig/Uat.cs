namespace Palmtree.Api.Sso.Endpoint.Msmq.EnvironmentSpecificConfig
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;

    public class Uat : IEnvironmentSpecificConfig
    {
        //for security we don't check these values into source control
        public IApplicationConfig Variables => JsonConvert.DeserializeObject<ApplicationConfiguration>(Environment.GetEnvironmentVariable("ApplicationConfig"));

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "Development")
                                                           .Enrich.WithProperty("Application", "BulldogApi")
                                                           .Enrich.WithExceptionDetails();

            SelfLog.Enable(msg => Trace.TraceError(msg)); //when seq connection fails azure trace logs will pick this up
        }
    }
}