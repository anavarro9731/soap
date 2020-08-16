// namespace Soap.Pf.HttpEndpointBase.Controllers
// {
//     public class HomeController : Controller
//     {
//         private readonly string _ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
//                                                 .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
//                                                 .ToString();
//
//         private readonly string _version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
//
//         [HttpGet("health", Name = nameof(GetHealthCheck))]
//         public JsonResult GetHealthCheck()
//         {
//             {
//                 var appConfig = (IApplicationConfig)HttpContext.RequestServices.GetService(typeof(IApplicationConfig));
//
//                 var healthCheck = new
//                 {
//                     healthCheckedAt = DateTime.UtcNow,
//                     applicationName = appConfig.ApplicationName,
//                     version = this._version,
//                     machineName = Environment.MachineName,
//                     ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
//                                    .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
//                                    .ToString()
//                 };
//
//                 return new JsonResult(
//                     healthCheck,
//                     new JsonSerializerSettings
//                     {
//                         Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver()
//                     });
//             }
//         }
//
//         [HttpGet("/", Name = nameof(GetIndex))]
//         [Produces("text/html")]
//         public string GetIndex()
//         {
//             {
//                 var appConfig = (IApplicationConfig)HttpContext.RequestServices.GetService(typeof(IApplicationConfig));
//                 var seqLogsConfig = (ISeqLoggingConfig)HttpContext.RequestServices.GetService(typeof(ISeqLoggingConfig));
//
//                 var getHealthCheckUrl = Url.Link(nameof(GetHealthCheck), null);
//                 var getCommandSchemaUrl = Url.Link(nameof(CommandController.GetCommandSchema), null);
//                 var getQuerySchemaUrl = Url.Link(nameof(QueryController.GetQuerySchema), null);
//
//                 var html = new StringBuilder().Append(BuildHeaderSectionHtml(appConfig))
//                                               .Append(GetEndpointDetailsSectionHtml(appConfig))
//                                               .Append(BuildHealthCheckSectionHtml(getHealthCheckUrl))
//                                               .Append(BuildLogsSectionHtml(seqLogsConfig))
//                                               .Append(BuildMessageSchemaSectionHtml(getCommandSchemaUrl, getQuerySchemaUrl))
//                                               .Append(BuildFooterSectionHtml())
//                                               .ToString();
//
//                 return html;
//             }
//
//             #region HTML Builder Methods
//
//             string BuildHeaderSectionHtml(IApplicationConfig appConfig) =>
//                 $@"
// <html>
// <body>
//     <h1>{appConfig.ApplicationName} {this._version}</h1>";
//
//             string GetEndpointDetailsSectionHtml(IApplicationConfig appConfig) =>
//                 $@"
//     <p>
//         Application Name: <strong>{appConfig.ApplicationName}</strong> <br />
//         Application Version: <strong>{appConfig.ApplicationVersion}</strong> <br />
//         Version: <strong>{this._version}</strong> <br />
//         Machine Name: <strong>{Environment.MachineName}</strong> <br />
//         IP Address: <strong>{this._ipAddress}</strong> <br />
//         HTTP API Endpoint: <strong>{appConfig.ApiEndpointSettings.HttpEndpointUrl}</strong> <br />
//         MSMQ API Endpoint: <strong>{appConfig.ApiEndpointSettings.MsmqEndpointAddress}</strong> <br />
//     </p>";
//
//             string BuildHealthCheckSectionHtml(string getHealthCheckUrl) =>
//                 $@"
//     <p>
//         Health Check: <strong><a href='{getHealthCheckUrl}' target='_blank'>{getHealthCheckUrl}</a></strong>
//     </p>";
//
//             string BuildLogsSectionHtml(ISeqLoggingConfig seqLogsConfig) =>
//                 seqLogsConfig == null
//                     ? string.Empty
//                     : $@"
//     <p>
//         Logs: <strong><a href='{seqLogsConfig.ServerUrl}' target='_blank'>{seqLogsConfig.ServerUrl}</a></strong>
//     </p>";
//
//             string BuildMessageSchemaSectionHtml(string getCommandSchemaUrl, string getQuerySchemaUrl) =>
//                 $@"
//     <p>
//         CommandToForward Schemas: <strong><a href='{getCommandSchemaUrl}' target='_blank'>{getCommandSchemaUrl}</a></strong> <br /> 
//         Query Schemas: <strong><a href='{getQuerySchemaUrl}' target='_blank'>{getQuerySchemaUrl}</a></strong>
//     </p>";
//
//             string BuildFooterSectionHtml() =>
//                 @"
// </body>
// </html>";
//
//             #endregion
//         }
//     }
// }