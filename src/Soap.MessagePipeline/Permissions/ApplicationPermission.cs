namespace Soap.MessagePipeline.Permissions
{
    using System;
    using Newtonsoft.Json;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.Utility.PureFunctions;

    public class ApplicationPermission : IEquatable<ApplicationPermission>, IApplicationPermission
    {
        [JsonConstructor]
        protected ApplicationPermission(Guid id, string permissionName, Guid permissionRequiredToAdministerThisPermission, int displayOrder)
        {
            Id = id;
            PermissionName = permissionName;
            PermissionRequiredToAdministerThisPermission = permissionRequiredToAdministerThisPermission;
            DisplayOrder = displayOrder;
        }

        public int DisplayOrder { get; set; }

        public Guid Id { get; set; }

        public string PermissionName { get; set; }

        public Guid PermissionRequiredToAdministerThisPermission { get; set; }

        public static ApplicationPermission Create(Guid id, string permissionName, Guid permissionRequiredToAdministerThisPermission, int displayOrder = 99)
        {
            return new ApplicationPermission(id, permissionName, permissionRequiredToAdministerThisPermission, displayOrder);
        }

        public static bool operator ==(ApplicationPermission a, ApplicationPermission b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            // Return true if the fields match:
            return a.PropertiesAreEqual(b);
        }

        public static bool operator !=(ApplicationPermission a, ApplicationPermission b)
        {
            return !(a == b);
        }

        public void AuthoriseUser(IUserWithPermissions user)
        {
            Guard.Against(!user.HasPermission(this), "User not authorized to perform this action. You require the " + PermissionName + " permission .");
        }

        public override bool Equals(object obj)
        {
            //if the object passed is null return false;
            if (ReferenceEquals(null, obj)) return false;

            //if the objects are the same instance return true
            if (ReferenceEquals(this, obj)) return true;

            //if the objects are of different types return false
            if (obj.GetType() != GetType()) return false;

            //check on property equality
            return PropertiesAreEqual((ApplicationPermission)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        bool IEquatable<ApplicationPermission>.Equals(ApplicationPermission other)
        {
            return Equals(other);
        }

        protected bool PropertiesAreEqual(ApplicationPermission other)
        {
            return Id.Equals(other.Id);
        }
    }
}
