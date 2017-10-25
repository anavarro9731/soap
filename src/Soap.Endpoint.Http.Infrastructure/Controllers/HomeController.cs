namespace Soap.Endpoint.Http.Infrastructure.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;

    public class HomeController : Controller
    {
        private readonly IEnumerable<ConnectionStringSettings> _connectionStringSettings = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>();

        private readonly string _ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
                                                .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                                .ToString();

        private readonly string _version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        [HttpGet("health", Name = nameof(GetHealthCheck))]
        public JsonResult GetHealthCheck()
        {
            {
                var appConfig = (IApplicationConfig)HttpContext.RequestServices.GetService(typeof(IApplicationConfig));

                var healthCheck = new
                {
                    healthCheckedAt = DateTime.UtcNow,
                    applicationName = appConfig.ApplicationName,
                    version = this._version,
                    machineName = Environment.MachineName,
                    ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName()).Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString(),
                    databaseConnections = this._connectionStringSettings.Select(
                                                  cs =>
                                                      {
                                                      var available = false;
                                                      string error = null;

                                                      try
                                                      {
                                                          available = CheckAvailability(cs.ConnectionString);
                                                      }
                                                      catch (Exception ex)
                                                      {
                                                          error = ex.Message;
                                                      }

                                                      return new
                                                      {
                                                          name = cs.Name,
                                                          available,
                                                          error
                                                      };
                                                      })
                                              .OrderByDescending(x => x.available)
                                              .ThenBy(x => x.name)
                                              .ToList()
                };
                return new JsonResult(
                    healthCheck,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }

            bool CheckAvailability(string connectionString, int connectionTimeoutSeconds = 5)
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new Exception($"SQL connection string is empty");
                }

                var builder = new SqlConnectionStringBuilder
                {
                    ConnectionString = connectionString,
                    ConnectTimeout = connectionTimeoutSeconds
                };

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    if (connection.State != ConnectionState.Open)
                    {
                        throw new Exception($"SQL connection state is {connection.State}");
                    }

                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        var result = (int)command.ExecuteScalar();

                        var available = result == 1;

                        return available;
                    }
                }
            }
        }

        [HttpGet("/", Name = nameof(GetIndex))]
        [Produces("text/html")]
        public string GetIndex()
        {
            {
                var appConfig = (IApplicationConfig)HttpContext.RequestServices.GetService(typeof(IApplicationConfig));
                var seqLogsConfig = (ISeqLoggingConfig)HttpContext.RequestServices.GetService(typeof(ISeqLoggingConfig));
                var docsConfig = (IDocumentationConfig)HttpContext.RequestServices.GetService(typeof(IDocumentationConfig));

                var getHealthCheckUrl = Url.Link(nameof(GetHealthCheck), null);
                var getCommandSchemaUrl = Url.Link(nameof(CommandController.GetCommandSchema), null);
                var getQuerySchemaUrl = Url.Link(nameof(QueryController.GetQuerySchema), null);

                var html = new StringBuilder().Append(BuildHeaderSectionHtml(appConfig))
                                              .Append(GetEndpointDetailsSectionHtml(appConfig))
                                              .Append(BuildHealthCheckSectionHtml(getHealthCheckUrl))
                                              .Append(BuildLogsSectionHtml(seqLogsConfig))
                                              .Append(BuildDocumentationSectionHtml(docsConfig))
                                              .Append(BuildMessageSchemaSectionHtml(getCommandSchemaUrl, getQuerySchemaUrl))
                                              .Append(BuildFooterSectionHtml())
                                              .ToString();

                return html;
            }

            #region HTML Builder Methods

            string BuildHeaderSectionHtml(IApplicationConfig appConfig)
            {
                return $@"
<html>
<body>
    <h1>{appConfig.ApplicationName} {this._version}</h1>";
            }

            string GetEndpointDetailsSectionHtml(IApplicationConfig appConfig)
            {
                return $@"
    <p>
        Application Name: <strong>{appConfig.ApplicationName}</strong> <br />
        Version: <strong>{this._version}</strong> <br />
        Machine Name: <strong>{Environment.MachineName}</strong> <br />
        IP Address: <strong>{this._ipAddress}</strong> <br />
        HTTP API Endpoint: <strong>{appConfig.ApiServerSettings.HttpEndpointUrl}</strong> <br />
        MSMQ API Endpoint: <strong>{appConfig.ApiServerSettings.MsmqEndpointAddress}</strong> <br />
    </p>";
            }

            string BuildHealthCheckSectionHtml(string getHealthCheckUrl)
            {
                return $@"
    <p>
        Health Check: <strong><a href='{getHealthCheckUrl}' target='_blank'>{getHealthCheckUrl}</a></strong>
    </p>";
            }

            string BuildLogsSectionHtml(ISeqLoggingConfig seqLogsConfig)
            {
                return seqLogsConfig == null
                           ? string.Empty
                           : $@"
    <p>
        Logs: <strong><a href='{seqLogsConfig.SeqLogsServerUrl}' target='_blank'>{seqLogsConfig.SeqLogsServerUrl}</a></strong>
    </p>";
            }

            string BuildDocumentationSectionHtml(IDocumentationConfig docsConfig)
            {
                return docsConfig == null
                           ? string.Empty
                           : $@"
    <p>
        Documentation: <strong><a href='{docsConfig.DocumentationUrl}' target='_blank'>{docsConfig.DocumentationUrl}</a></strong>
    </p>";
            }

            string BuildMessageSchemaSectionHtml(string getCommandSchemaUrl, string getQuerySchemaUrl)
            {
                return $@"
    <p>
        Command Schemas: <strong><a href='{getCommandSchemaUrl}' target='_blank'>{getCommandSchemaUrl}</a></strong> <br /> 
        Query Schemas: <strong><a href='{getQuerySchemaUrl}' target='_blank'>{getQuerySchemaUrl}</a></strong>
    </p>";
            }

            string BuildFooterSectionHtml()
            {
                return $@"
</body>
</html>";
            }

            #endregion
        }
    }
}