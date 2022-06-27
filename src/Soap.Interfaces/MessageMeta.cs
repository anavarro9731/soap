namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta(
            (DateTime receivedAt, long receivedAtTick) receivedAt,
            IdentityClaims identityClaims,
            IUserProfile userProfile,
            AuthLevel authLevel,
            Guid messageId)
        {
            ReceivedAt = receivedAt;
            IdentityClaimsOrNull = identityClaims;
            UserProfileOrNull = userProfile;
            AuthLevel = authLevel;
            MessageId = messageId;
        }

        public MessageMeta()
        {
        }

        [JsonProperty]
        public IdentityClaims IdentityClaimsOrNull { get; internal set; }

        [JsonProperty]
        public Guid MessageId { get; internal set; }

        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)] //* dont persist as IUserProfile
        public IUserProfile UserProfileOrNull { get; internal set; }

        [JsonProperty]
        public AuthLevel AuthLevel { get; internal set; }

        public bool UserHasApiPermission(ApiPermission apiPermission)
        {
            return IdentityClaimsOrNull?.ApiPermissions.Exists(a => a == apiPermission) ?? false;
        }

        public bool UserHasApiPermissionAndScope<ScopeType>(Guid scopeId, ApiPermission apiPermission)
        {
            return UserHasApiPermission(apiPermission) && UserHasScope<ScopeType>(scopeId);
        }

        //* useful for checking for customer developer permissions
        public bool UserHasDeveloperPermission(string developerPermission)
        {
            return IdentityClaimsOrNull?.ApiPermissions.SelectMany(x => x.DeveloperPermissions).Contains(developerPermission) ?? false;
        }

        /// <summary>
        /// Need to be careful with the use of this if your system is allowing custom roles, better to check if user has API permissions if you are
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool UserHasRole(Role role)
        {
            return IdentityClaimsOrNull?.Roles.SingleOrDefault(x => x.RoleKey == role.Key) != null;
        }

        /// <summary>
        /// Need to be careful with the use of this if your system is allowing custom roles, better to check if user has API permissions if you are
        /// </summary>
        /// <param name="scopeId"></param>
        /// <param name="roleId"></param>
        /// <typeparam name="ScopeType"></typeparam>
        /// <returns></returns>
        public bool UserHasRoleAndScope<ScopeType>(Guid scopeId, Role role)
        {
            return UserHasRole(role) && UserHasScope<ScopeType>(scopeId);
        }

        public bool UserHasScope<ScopeType>(Guid scopeId)
        {
            return (IdentityClaimsOrNull?.Roles.SelectMany(x => x.ScopeReferences)
                               .Any(s => s.AggregateType == typeof(ScopeType).FullName && s.AggregateId == scopeId) ?? false);
        }
    }
}