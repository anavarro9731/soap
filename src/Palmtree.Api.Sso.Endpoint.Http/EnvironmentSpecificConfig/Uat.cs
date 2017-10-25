namespace Palmtree.Api.Sso.Endpoint.Http.EnvironmentSpecificConfig
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Newtonsoft.Json;
    using Palmtree.Api.Sso.Domain.Logic;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Soap.Endpoint.Http.Infrastructure;
    using Soap.Interfaces;

    public class Uat : IHttpEnvironmentSpecificConfiguration
    {
        public IApplicationConfig Variables =>
            //for security we don't check these values into source control
            JsonConvert.DeserializeObject<ApplicationConfiguration>(Environment.GetEnvironmentVariable("ApplicationConfig"));

        public void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder)
        {
            policyBuilder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().AllowCredentials().Build();
        }

        public void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "Development")
                                                           .Enrich.WithProperty("Application", Variables.ApplicationName)
                                                           .Enrich.WithExceptionDetails()
                                                           .WriteTo.ColoredConsole()
                                                           .WriteTo.Seq("http://13.81.4.220", apiKey: "1OSAbhW4o6ekOctXiwyk");

            SelfLog.Enable(msg => Trace.TraceError(msg)); //when seq connection fails azure trace logs will pick this up
        }
    }
}