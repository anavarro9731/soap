namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, IdentityPermissions identityPermissions, IUserProfile userProfile, Guid messageId)
        {
            ReceivedAt = receivedAt;
            IdentityPermissionsOrNull = identityPermissions;
            UserProfileOrNull = userProfile;
            MessageId = messageId;
        }

        public MessageMeta() {}
        
        [JsonProperty]
        public Guid MessageId { get; internal set; }
        
        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty]
        public IdentityPermissions IdentityPermissionsOrNull { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)] //* dont persist as IUserProfile
        public IUserProfile UserProfileOrNull { get; internal set; }
    }
}
