
namespace Soap.Interfaces
{
    using System;

    public interface IUserProfile : IHaveAuth0Id
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
