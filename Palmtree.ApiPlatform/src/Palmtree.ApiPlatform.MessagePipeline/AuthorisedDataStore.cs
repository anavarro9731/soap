namespace Palmtree.ApiPlatform.MessagePipeline.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    /// <summary>
    ///     A Datastore that is user context aware and will check if the users permissions have matching
    ///     scope to the data objects they are requesting.
    ///     Does the user X have Y permission against scope Z
    ///     Very useful in multi-tenanted dbs
    /// </summary>
    public class AuthorisedDataStore
    {
        public AuthorisedDataStore(IDataStore dataStore, IUserWithPermissions user)
        {
            DataStore = dataStore;
            FilteredBy = user;
        }

        public IDataStore DataStore { get; }

        public IUserWithPermissions FilteredBy { get; }

        // a user has permission scoped against some aggregate, and each aggregate is also scope against another aggregate
        // this checks to see that the user has a particular permission with a particular scope
        // which matches the scope of each aggregate in the list.
        // an aggregates scope is a collection of id of other objects and possibly itself in cases where
        // you want to control access to a single aggrgate on a per user basis as opposed to giving access via
        // another aggregate such as "Department, Customer, Role, etc).
        public static void AuthorizeDataAccess(IUserWithPermissions user, IDataPermission permission, IEnumerable<IHaveScope> objectsBeingAuthorized)
        {
            if (user.HasPermission(permission)) //if the user has the permission in question
            {
                foreach (var objectQueried in objectsBeingAuthorized)
                {
                    //if the object queried is not scope, return it, it is unsecured
                    if (objectQueried.ScopeReferences == null || objectQueried.ScopeReferences.Count == 0) continue;

                    //if the user's permission is scoped to the same scope as the object being queried allow it.
                    var usersScopedPermission = (IDataPermission)user.Permissions.Single(x => Equals(x, permission));
                    if (usersScopedPermission.PermissionScope.Intersect(objectQueried.ScopeReferences).Any()) continue;

                    //otherwise don't!
                    throw new SecurityException(
                        "User not authorized to perform this action. You require the " + permission.PermissionName
                        + " permission scoped to one of the following which objects you do not have: " + objectQueried
                            .ScopeReferences.Select(s => $"{s.ScopeObjectType} - {s.ScopeObjectId}")
                            .Aggregate((a, b) => $"[{a}] [{b}]"));
                }
            }
        }

        public static void AuthorizeDataAccess(IUserWithPermissions user, IDataPermission permission, IHaveScope objectBeingAuthorized)
        {
            AuthorizeDataAccess(
                user,
                permission,
                new[]
                {
                    objectBeingAuthorized
                });
        }

        public async Task CommitChanges()
        {
            await DataStore.CommitChanges().ConfigureAwait(false);
        }

        public void Dispose()
        {
            DataStore.Dispose();
        }
    }
}
