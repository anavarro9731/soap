

//
// namespace Soap.Pf.HttpEndpointBase.Controllers
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using System.Net;
//     using System.Net.Sockets;
//     using System.Reflection;
//     using System.Threading.Tasks;
//     using Newtonsoft.Json;
//     using Serilog;
//     using Soap.Interfaces;
//     using Soap.Interfaces.Messages;
//     using Soap.MessagePipeline;
//     using Soap.MessagePipeline.Context;
//     using Soap.MessagePipeline.MessagePipeline;
//     using Soap.PfBase.Api;
//     using Soap.Utility.Functions.Extensions;
//
//     public static class InitialiseFunctions
//     {
//
//         private static async Task<string> GetToken()
//         {
//             try
//             {
//                 IAzure azure = Azure.Authenticate(credFile).WithDefaultSubscription();
//                 return result.AccessToken;
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine("Could not get a token...");
//                 Console.WriteLine(e.Message);
//                 throw e;
//             }
//         }
//         
//

//     }
// }