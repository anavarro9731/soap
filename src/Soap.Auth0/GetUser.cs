namespace Soap.Auth0
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Soap.Config;
    using Soap.Interfaces;

    public static partial class Auth0Functions
    {
        public static async Task<TUserProfile> GetUserProfile<TUserProfile>(
            ApplicationConfig applicationConfig,
            DataStore dataStore,
            ApiIdentity apiIdentity) where TUserProfile : class, IHaveAuth0Id, IUserProfile, IAggregate, new()
        {
            var auth0User = await GetUserProfileFromIdentityServer(applicationConfig, apiIdentity.Auth0Id);

            var user = (await dataStore.Read<TUserProfile>(x => x.Auth0Id == auth0User.UserId)).SingleOrDefault();

            if (user == null)
            {
                var newUser = new TUserProfile
                {
                    Auth0Id = auth0User.UserId,
                    Email = auth0User.Email,
                    FirstName = auth0User.FirstName,
                    LastName = auth0User.LastName
                };

                return await dataStore.Create(newUser);
            }
            else
            {
                return (await dataStore.UpdateWhere<TUserProfile>(
                    u => u.Auth0Id == user.Auth0Id,
                    x =>
                        {
                        x.Email = auth0User.Email;
                        x.FirstName = auth0User.FirstName;
                        x.LastName = auth0User.LastName;
                        })).Single();
            }
        }
    }
}
