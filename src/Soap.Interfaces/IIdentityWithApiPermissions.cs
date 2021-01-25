namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityWithApiPermissions
    {
        List<string> ApiPermissions { get; set; }
    }
}
