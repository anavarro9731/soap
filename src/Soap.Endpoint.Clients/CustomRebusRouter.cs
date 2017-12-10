namespace Soap.Pf.EndpointClients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Rebus.Logging;
    using Rebus.Messages;
    using Rebus.Routing;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.ClientServerMessaging.Routing;

    public class CustomRebusRouter : IRouter
    {
        private IEnumerable<MessageRoute_Msmq> routes;

        public CustomRebusRouter(IEnumerable<MessageRoute_Msmq> routes, IRebusLoggerFactory rebusLoggerFactory)
        {
            {
                SetRoutes();

                LogRoutes();
            }

            void SetRoutes()
            {
                // ReSharper disable once PossibleMultipleEnumeration
                Guard.Against(routes == null || !routes.Any(), "No Routing Definitions provided");

                // ReSharper disable once PossibleMultipleEnumeration
                this.routes = routes;
            }

            void LogRoutes()
            {
                rebusLoggerFactory.GetLogger<CustomRebusRouter>().Info(this.routes.RoutesAsLoggableString());
            }
        }

        //.. get the address to send to in normal circumstances
        public Task<string> GetDestinationAddress(Message message)
        {
            Guard.Against(message?.Body == null, "Message cannot be null");

            var apiMessage = message.Body as IApiMessage;

            Guard.Against(apiMessage == null, $"Message must be of type {nameof(IApiMessage)} when using {nameof(CustomRebusRouter)}");

            var route = this.routes.FirstOrDefault(r => r.CanRouteMessage(apiMessage));

            Guard.Against(route == null, $"Could not find a matching route for message of type {apiMessage.GetType().Name}");

            return Task.FromResult(route.MsmqEndpointAddress.ToString());
        }

        public Task<string> GetOwnerAddress(string topic)
        {
            throw new NotImplementedException();
        }
    }
}
