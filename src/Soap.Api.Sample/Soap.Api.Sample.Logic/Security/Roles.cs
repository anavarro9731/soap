namespace Soap.Api.Sample.Logic.Security
{
    using System.Collections.Generic;
    using Soap.Interfaces;

    public partial class Roles
    {
      
        public static readonly Role SampleRole = new Role("builtin-messages/execute", "Send BuiltIn Messages")
        {
            Description = "This is a sample role.",
            ApiPermissions = new List<ApiPermission>
            {
                ApiPermissions.BuiltInMessages    
            }
        };

    }
}