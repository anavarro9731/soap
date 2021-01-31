namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, IdentityPermissions identityPermissions, string userId, string auth0Id)
        {
            ReceivedAt = receivedAt;
            IdentityPermissions = identityPermissions;
            Auth0Id = auth0Id;
            UserId = userId;
        }

        public MessageMeta() {}
        
        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IdentityPermissions IdentityPermissions { get; internal set; }

        [JsonProperty]
        public string Auth0Id { get; internal set; }

        [JsonProperty]
        public string UserId { get; internal set; }
    }
}
