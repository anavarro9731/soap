namespace Soap.Auth0
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using global::Auth0.ManagementApi;
    using global::Auth0.ManagementApi.Models;
    using Microsoft.IdentityModel.Tokens;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public static partial class Auth0Functions
    {
        public static class Profiles
        {
            public static async Task<TUserProfile> GetUserProfileOrNull<TUserProfile>(
                ApplicationConfig applicationConfig,
                DataStore dataStore,
                string idToken) where TUserProfile : class, IHaveAuth0Id, IUserProfile, IAggregate, new()
            {
                if (idToken == null) return null;

                var auth0User = await GetUserProfileFromIdentityToken(idToken, applicationConfig);

                var user = (await dataStore.Read<TUserProfile>(x => x.Auth0Id == auth0User.UserId)).SingleOrDefault();

                if (user == null)
                {
                    var newUser = new TUserProfile
                    {
                        Auth0Id = auth0User.UserId,
                        Email = auth0User.Email,
                        FirstName = FirstName(auth0User),
                        LastName = LastName(auth0User)
                    };

                    return await dataStore.Create(newUser);
                }

                return (await dataStore.UpdateWhere<TUserProfile>(
                            u => u.Auth0Id == user.Auth0Id,
                            x =>
                                {
                                x.Email = auth0User.Email;
                                x.FirstName = FirstName(auth0User);
                                x.LastName = LastName(auth0User);
                                })).Single();

                string FirstName(User auth0User) =>
                    !string.IsNullOrEmpty(auth0User.FirstName)
                        ? auth0User.FirstName
                        : (auth0User.FullName.Contains(' ') ? auth0User.FullName.SubstringBeforeLast(' ') : auth0User.FullName);

                string LastName(User auth0User) =>
                    !string.IsNullOrEmpty(auth0User.LastName) ? auth0User.LastName : auth0User.FullName.SubstringAfterLast(' ');
                
            }

            private static async Task<User> GetUserProfileFromIdentityServer(ApplicationConfig applicationConfig, string auth0Id)
            {
                string mgmtToken = null;
                ManagementApiClient client = null;

                await Tokens.GetManagementApiToken(applicationConfig, v => mgmtToken = v);

                GetManagementApiClient(mgmtToken, applicationConfig, v => client = v);

                var user = await client.Users.GetAsync(auth0Id);

                return user;
            }

            private static async Task<User> GetUserProfileFromIdentityToken(string idToken, ApplicationConfig applicationConfig)
            {
                var openIdConfig = await GetOpenIdConfig($"{applicationConfig.Auth0TenantDomain}");

                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidIssuer = openIdConfig.Issuer
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    //* will validate formation and signature by default

                    tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);
                    var tokenWithClaims = validatedToken as JwtSecurityToken;

                    return new User
                    {
                        Email = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "email")?.Value,
                        FirstName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "given_name")?.Value,
                        LastName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "family_name")?.Value,
                        FullName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "name")?.Value,
                        NickName = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "nickname")?.Value,
                        UserId = tokenWithClaims.Claims.SingleOrDefault(x => x.Type == "sub")?.Value
                    };
                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new CircuitException("The id token is expired.", ex);
                }
                catch (Exception e)
                {
                    throw new CircuitException("The id token is invalid.", e);
                }
            }
        }
    }
}