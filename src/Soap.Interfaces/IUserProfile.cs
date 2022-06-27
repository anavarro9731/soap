
namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IUserProfile : IdaamProviderProfile, IHaveAUniqueId
    {
        
    }

    public interface IdaamProviderProfile : IHaveIdaamProviderId
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
