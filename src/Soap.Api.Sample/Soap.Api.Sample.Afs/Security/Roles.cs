namespace Soap.Api.Sample.Afs.Security
{
    using System.Collections.Generic;
    using Soap.Interfaces;

    public class Roles
    {
        public static readonly Role TestDataAnalyst = new Role("test-data-analyst", "Test Data Analyst")
        {
            Description = "Access to View All Test Data",
            ApiPermissions = new List<ApiPermission> { ApiPermissions.TestData__Read }
        };

        public static readonly Role TestDataManager = new Role("test-data-manager", "Test Data Manager")
        {
            Description = "Access to Change All Test Data",
            ApiPermissions = new List<ApiPermission> { ApiPermissions.TestData__Read, ApiPermissions.TestData__Write }
        };
        
    }
}