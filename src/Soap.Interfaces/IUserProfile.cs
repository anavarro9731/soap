
namespace Soap.Interfaces
{
    using System;
    using DataStore.Interfaces.LowLevel;

    public interface IUserProfile : IHaveAuth0Id, IHaveAUniqueId
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
