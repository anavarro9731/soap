namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, IdentityPermissions identityPermissions, IUserProfile userProfile)
        {
            ReceivedAt = receivedAt;
            IdentityPermissionsOrNull = identityPermissions;
            UserProfileOrNull = userProfile;
        }

        public MessageMeta() {}
        
        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IdentityPermissions IdentityPermissionsOrNull { get; internal set; }

        [JsonProperty]
        public IUserProfile UserProfileOrNull { get; internal set; }
    }
}
