//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Security
{
    using System.Collections.Generic;
    using Azure.Storage.Blobs.Models;
    using Soap.Interfaces;

    public partial class Roles
    {
        public static readonly Role TestDataAnalyst = new Role("test-data-analyst", "Test Data Analyst")
        {
            Description = "Access to View All Test Data",
            ApiPermissions = new List<ApiPermission> { ApiPermissions.TestData__Read }
        };

        public static readonly Role TestDataManager = new Role("test-data-manager", "Test Data Manager")
        {
            Description = "Access to Change All Test Data",
            ApiPermissions = new List<ApiPermission>
            {
                ApiPermissions.TestData__Read, 
                ApiPermissions.TestData__Write
            }
        };
        
        public static readonly Role TestDataAdmin = new Role("test-data-admin", "Test Data Admin")
        {
            Description = "Access to Change All Data",
            ApiPermissions = new List<ApiPermission>
            {
                ApiPermissions.TestData__Read, 
                ApiPermissions.TestData__Write,
                ApiPermissions.TestData__RestrictedOperations,
                ApiPermissions.TestData__RestrictedOperations
            }
        };
        
        public static readonly Role SystemAdmin = new Role("sys-admin", "System Admin")
        {
            Description = "Access to Change All Data",
            ApiPermissions = new List<ApiPermission>
            {
                ApiPermissions.Admin
            }
        };

    }
}