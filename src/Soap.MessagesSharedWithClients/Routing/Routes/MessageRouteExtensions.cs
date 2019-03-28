namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class MessageRouteExtensions
    {
        public static string RoutesAsLoggableString(this IEnumerable<MessageRoute> routes)
        {
            var routesAsLoggableString = string.Empty;

            routes.ToList().ForEach(r => routesAsLoggableString += r.ToString() + Environment.NewLine);

            return routesAsLoggableString;
        }
    }
}