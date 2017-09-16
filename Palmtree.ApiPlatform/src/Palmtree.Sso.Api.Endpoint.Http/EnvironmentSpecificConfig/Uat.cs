namespace Palmtree.Sample.Api.Endpoint.Http.EnvironmentSpecificConfig
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Exceptions;
    using Palmtree.ApiPlatform.Endpoint.Http.Infrastructure;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.Sample.Api.Domain.Logic;

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
