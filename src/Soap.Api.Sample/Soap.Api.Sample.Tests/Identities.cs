namespace Soap.Api.Sample.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share instances and have concurrency problems if
        one test modifies this.  the identity sent to the test, is looked up
        by the auth scheme from the copy held in the inmemoryidaamprovider so any changes you 
        make to it before you handle it (e.g. in the beforerunhook will be acknowledged).
         */

    public static class Identities
    {
        public static TestIdentity JaneDoeNoPermissions =>
            new TestIdentity(
                new List<RoleInstance>(),
                new UserProfile
                {
                    IdaamProviderId = Ids.JaneDoeWithNoPermissions.ToIdaam(),
                    Email = "janedoe@mycomputer.com",
                    id = Ids.JaneDoeWithNoPermissions,
                    FirstName = "Jane",
                    LastName = "Doe"
                });

        public static TestIdentity JohnDoeAllPermissions =>
            new TestIdentity(
                new SecurityInfo().BuiltInRoles.Select(
                                      x => new RoleInstance
                                      {
                                          RoleKey = x.Key,
                                          ScopeReferences = new List<AggregateReference>
                                          {
                                              /* IN PRODUCTION CODE THIS WOULD BE POPULATED
                                               AND THE DATABASE PERMISSIONS WOULD ALWAYS BE POPULATED
                                               FROM THIS. SINCE WE DON'T KNOW WHAT POSSIBLE DATA WILL
                                               BE THERE IN THE TEST WE ADD A WILDCARD PERMISSION USING
                                               A SPECIALIDS */
                                              new AggregateReference(Ids.UseDbPermissionScopeWildcard)
                                          }
                                      })
                                  .ToList(),
                new UserProfile
                {
                    IdaamProviderId = Ids.JohnDoeWithAllPermissions.ToIdaam(),
                    Email = "johndoe@mycomputer.com",
                    id = Ids.JohnDoeWithAllPermissions,
                    FirstName = "John",
                    LastName = "Doe"
                });
        
        public static TestIdentity WithApiPermissions =>
            new TestIdentity(
                new SecurityInfo().BuiltInRoles.Select(
                                      x => new RoleInstance
                                      {
                                          RoleKey = x.Key
                                      })
                                  .ToList(),
                new UserProfile
                {
                    IdaamProviderId = Ids.ApiPermissionsOnly.ToIdaam(),
                    Email = "apipermissions@mycomputer.com",
                    id = Ids.ApiPermissionsOnly,
                    FirstName = "Api",
                    LastName = "Permissions"
                });
        
        public static List<TestIdentity> TestIdentities
        {
            get
            {
                var identities = typeof(Identities).GetProperties()
                                                   .Where(x => x.PropertyType == typeof(TestIdentity))
                                                   .Select(x => x.GetValue(null, null))
                                                   .Select(y => y.DirectCast<TestIdentity>())
                                                   .ToList();

                return identities;
            }
        }
    }
}