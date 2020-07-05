namespace Sample.Logic
{
    using System;
    using Soap.Interfaces;

    public class Permissions : Soap.Interfaces.ApiPermissionList
    {
        private static readonly Guid UpdateSystemSettingsId = Guid.Parse("fffa42e9-4d4c-4a27-bc4c-4720604eaa26");

        public static readonly ApiPermission UpdateSystemSettings = new ApiPermission(
            UpdateSystemSettingsId,
            "Update System Settings",
            Guid.Empty,
            100);
    }
}