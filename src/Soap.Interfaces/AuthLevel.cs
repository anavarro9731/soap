namespace Soap.Interfaces
{
    using CircuitBoard;

    public class AuthLevel : TypedEnumeration<AuthLevel>
    {
        /// <summary>
        /// There is no authentication or authorisation.
        /// </summary>
        public static AuthLevel None = Create("NONE", nameof(AuthLevel));
        
        public static AuthLevel AuthenticateOnly = Create("AUTHENTICATE-ONLY", nameof(AuthenticateOnly));
        
        /// <summary>
        /// You might use this in a single tenant scenario where you don't need to check
        /// the scope of the data being affected by the commands, but users can have different
        /// permissions on the API. You can use roles here to assign permissions and you can also
        /// give scopes to the roles which might be useful to partition access to data within
        /// a single tenant but you would have to check the relationship of the users scopes
        /// to the underlying data through custom code not using DataStore's builtin security apparatus.
        /// </summary>
        public static AuthLevel AuthoriseApiPermissions = Create("AUTHORISE-API", nameof(AuthoriseApiPermissions));

        /// <summary>
        /// You might use this in a multi-tenanted scenario where you want to be able to authorise
        /// calls to the database based on the corresponding scopes of a user's role(s) but you want
        /// to do so in a limited and opt-in way through the manual setting of the .AuthoriseFor() option
        /// when calling datastore methods.
        ///
        /// IMPORTANT: If you want to secure your data against more than one aggregate, and you
        /// want the relationships between them to be inferred (e.g. If an user has access to
        /// CompanyA then they will have access to all Sites in CompanyA without having to add
        /// the scopes for the Sites to the user's role(s) then you must provide the ReceiveMessage
        /// endpoint with a ScopeHierarchy object by passing a DataStoreOptions object to the
        /// Platform.HandleMessageFunction using the DataStoreOptions.WithSecurity(scopeHierarchy)
        /// configuration method.
        ///
        /// This AuthLevel will also enable API level permissions that will validate incoming messages
        /// against the permissions in the user's role. 
        /// </summary>
        public static AuthLevel AuthoriseApiAndDatabasePermissionsOptIn = Create("AUTHORISE-API+DBOPTIN", nameof(AuthoriseApiAndDatabasePermissionsOptIn));
        
        /// <summary>
        /// You would use this in a scenario where you want to use Roles with Role Scopes to control
        /// access to every piece of data in the system. Any database objects without a scope that are accessed
        /// within a ReceiveMessage endpoint will be inaccessible to the caller at runtime unless the
        /// aggregate(s) requested are marked with the [BypassSecurity] attribute or the call itself is
        /// made with the ByPassSecurity() option. Attempting to access data with no scope or no intersection
        /// between role scope and data scope will throw an error. This is an opt-out scenario vs the
        /// ApiAndDatabasePermission level which is effectively opt-in. 
        /// 
        /// This will also enable API level permissions that will validate incoming messages
        /// against the permissions in the user's role.
        /// </summary>
        public static AuthLevel AuthoriseApiAndDatabasePermissionsOptOut = Create("API+DBOPTOUT", nameof(AuthoriseApiAndDatabasePermissionsOptOut));
        
        
        /*
         * In all cases it not possible to use scope intersections as a filter on the data being returned or modified.
         * This is considered as creating a situation where it is too easy to hide problems in the data, and will
         * make it hard to debug. e.g. You send a command for A,B,C and received only A,B due to scope restrictions.
         *
         * In any cases where ApiPermissionEnabled == true you may use Meta.UserHasPermission or Meta.UserHasPermissionWithScope to make checks on CustomerDeveloperPermissions.
         */

        public bool Enabled => this != None;
        
        public bool AuthenticationRequired => ApiPermissionsRequired || this == AuthenticateOnly; 

        public bool ApiPermissionsRequired =>
            this == AuthoriseApiPermissions || this == AuthoriseApiAndDatabasePermissionsOptIn 
                                            || this == AuthoriseApiAndDatabasePermissionsOptOut;

        public bool DatabasePermissionRequired =>
            this == AuthoriseApiAndDatabasePermissionsOptIn 
            || this == AuthoriseApiAndDatabasePermissionsOptOut;
    }
}