namespace Soap.Interfaces.Messages
{
    using System;

    public class ApiPermission
    {
        public ApiPermission(
            Guid id,
            string permissionName,
            Guid permissionRequiredToAdministerThisPermission,
            int displayOrder = 99)
        {
            Id = id;
            PermissionName = permissionName;
            PermissionRequiredToAdministerThisPermission = permissionRequiredToAdministerThisPermission;
            DisplayOrder = displayOrder;
        }

        public ApiPermission()
        {
            //serialiser
        }

        public int DisplayOrder { get; set; }

        public Guid Id { get; set; }

        public string PermissionName { get; set; }

        public Guid PermissionRequiredToAdministerThisPermission { get; set; }

        public static bool operator ==(ApiPermission a, ApiPermission b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(ApiPermission a, ApiPermission b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ApiPermission)obj);
        }

        public override int GetHashCode() => Id.GetHashCode();

        protected bool Equals(ApiPermission other) => Id.Equals(other.Id);
    }
}