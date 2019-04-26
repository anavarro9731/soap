namespace Soap.Api.Sso.Domain.Logic.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.Entities;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.EndpointInfrastructure;

    public class UserOperations : Operations<User>
    {
        public class ErrorCodes : ErrorCode
        {
            public static ErrorCodes UsernameAlreadyTaken = ErrorCode.Create<ErrorCodes>(
                Guid.Parse("a57d03bc-84b0-4403-a98e-b790b4614b2f"),
                "This username is already taken.");
        }

        public async Task<User> AddDefaultUser()
        {
            {
                DetermineChange(out var user);

                await DataStore.Create(user);

                return user;
            }

            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(HardCodedMasterData.RootUser.Password);

                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = new User
                {
                    Email = HardCodedMasterData.RootUser.EmailAddress,
                    FullName = HardCodedMasterData.RootUser.FullName,
                    History = AccountHistory.Create(),
                    PasswordDetails = passwordDetails,
                    Permissions = new List<IApplicationPermission>(),
                    UserName = HardCodedMasterData.RootUser.UserName,
                    ActiveSecurityTokens = new List<SecurityToken>(),
                    id = HardCodedMasterData.RootUser.UserId,
                    TagIds = new List<Guid>(),
                    Status = FlaggedState.Create(User.UserStates.EmailConfirmed)
                };
            }
        }
         
        public async Task<User> AddFullyRegisteredUser(Guid? id, string email, string name, string username, string password)
        {
            {
                await Validate();

                DetermineChange(out var user);

                await DataStore.Create(user);

                return user;
            }


            async Task Validate()
            {
                Guard.Against(
                    await DataStoreReadWithoutEventReplay.CountActive<User>(x => x.UserName == email) > 0,
                    ErrorCodes.UsernameAlreadyTaken);
            }

            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(password);
                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = UserCreate(email, name, passwordDetails, null, username, id);
                user.Status = FlaggedState.Create(User.UserStates.EmailConfirmed);
            }
        }

        public async Task<User> AddPartiallyRegisteredUser(string email, string name, string password)
        {
            {
                await Validate();

                DetermineChange(out var user);

                await DataStore.Create(user);

                return user;
            }

            async Task Validate()
            {
                Guard.Against(
                    await DataStoreReadWithoutEventReplay.CountActive<User>(x => x.UserName == email) > 0,
                    ErrorCodes.UsernameAlreadyTaken);
            }
            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(password);
                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = UserCreate(email, name, passwordDetails, null, email);
            }
        }

        public async Task ChangeUserEmail(Guid userId, string newEmail)
        {
            await DataStore.UpdateById(userId, u => u.Email = newEmail);
        }

        public async Task AddTagToUser(Guid userId, Tag Tag)
        {
            {
                DetermineChange(out var change, Tag.id);

                await DataStore.UpdateById(userId, change);
            }

            void DetermineChange(out Action<User> change, Guid TagId)
            {
                change = u => u.TagIds.Add(TagId);
            }
        }

        public async Task<ClientSecurityContext> AuthenticateUser(Credentials credentials)
        {
            {
                var user = await FindUserWithMatchingUsername();

                Validate(user);

                DetermineChange(user, out var changes);

                await DataStore.UpdateById(user.id, changes.updateUser, true);

                return changes.response;
            }

            void Validate(User user)
            {


                Guard.Against(() => user == null, "Login Failed. 01", "User does not exist.");

                Guard.Against(() => user.Status.HasState(User.UserStates.AccountDisabled), "Your account is disabled please contact support");

                Guard.Against(
                    user.UserAccountIsLocked,
                    "Your have already entered an incorrect password too many times. Your account is now locked. Please try again in five minutes.");

                Guard.Against(user.UserName != credentials.Username, "These credentials are for a different user. Check code logic.");
            }

            async Task<User> FindUserWithMatchingUsername()
            {
                var matchingUsers = await DataStoreRead.ReadActive<User>(u => u.UserName == credentials.Username);

                return matchingUsers.SingleOrDefault();
            }

            void DetermineChange(User user, out (Action<User> updateUser, ClientSecurityContext response) changes)
            {
                
                if (user.NormalCredentialsMatch(Credentials.Create(credentials.Username, credentials.Password)))
                {
                    var securityToken = SecurityToken.Create(user.id, user.PasswordDetails.PasswordHash, DateTime.UtcNow, new TimeSpan(0, 15, 0), true);

                    changes = (u => Login(u, securityToken), ClientSecurityContext.Create(securityToken, user));
                }
                else
                {
                    changes = (IncrementInvalidLoginData, null);

                    Guard.Against(true, "Login Failed. 11", "Users normal credentials failed to match and no password reset was requested");
                }
            }

            void IncrementInvalidLoginData(User user)
            {
                user.History.LastInvalidLoginAttempt = DateTime.UtcNow;
                user.History.InvalidLoginCount = user.History.InvalidLoginCount + 1;
            }

            void Login(User user, SecurityToken securityToken)
            {
                user.History.InvalidLoginCount = 0;
                user.History.LastLoggedIn = DateTime.UtcNow;
                user.PasswordDetails.PasswordResetTokenExpiry = null;
                user.PasswordDetails.PasswordResetStatefulProcessId = null;
                if (!user.ActiveSecurityTokens.Contains(securityToken)) user.ActiveSecurityTokens.Add(securityToken);
            }
        }

        public async Task ConfirmEmail(Guid userId)
        {
            {
                DetermineChange(out var change);

                await DataStore.UpdateById(userId, change, true);
            }

            void DetermineChange(out Action<User> change)
            {
                change = u => u.Status.AddState(User.UserStates.EmailConfirmed);
            }
        }

        public async Task DisableUserAccount(Guid userId)
        {
            await DataStore.UpdateById(userId, u => u.Status.AddState(User.UserStates.AccountDisabled));
        }

        public async Task EnableUserAccount(Guid userId)
        {
            await DataStore.UpdateById(userId, u => u.Status.RemoveState(User.UserStates.AccountDisabled));
        }

        public async Task RequestPasswordReset(string email, Guid passwordResetStatefulProcessId)
        {
            {
                var user = (await DataStoreRead.ReadActive<User>(u => u.Email == email)).SingleOrDefault();

                Validate(user);

                DetermineChange(out var change);

                await DataStore.UpdateById(user.id, change, true);
            }

            void Validate(User user)
            {

                Guard.Against(user.AccountIsDisabled, "This account is disabled. Contact Support.");

                Guard.Against(() => user == null, "No account is associated with this email address");
            }

            void DetermineChange(out Action<User> change)
            {
                change = u =>
                    {
                    u.PasswordDetails.PasswordResetStatefulProcessId = passwordResetStatefulProcessId;
                    u.PasswordDetails.PasswordResetTokenExpiry = GeneratePasswordResetTokenExpiry();
                    };
            }

            DateTime GeneratePasswordResetTokenExpiry()
            {
                return DateTime.Now.Add(new TimeSpan(0, 2, 0, 0));
            }
        }

        public async Task<ClientSecurityContext> ResetPasswordFromEmail(string username, string newPassword, Guid statefulProcessId)
        {
            {
                var user = await Validate();

                CreateNewPasswordHash(out var newHash);

                CreateNewSecurityToken(newHash, user.id, out var newToken);

                DetermineChange(user, newHash, newToken, out var change);

                await DataStore.UpdateById(user.id, change, true);

                return ClientSecurityContext.Create(newToken, user);
            }

            async Task<User> Validate()
            {

                var requestingUser = (await DataStoreRead.Read<User>(user => user.UserName == username)).SingleOrDefault();

                Guard.Against(() => requestingUser == null, "Request failed", "User does not exist");

                Guard.Against(() => requestingUser.AccountIsDisabled(), "Account is Disabled");

                Guard.Against(() => !requestingUser.Active, "Account not found", "Account is deleted");

                Guard.Against(() => requestingUser.PasswordResetRequestHasExpired(), "Temporary Password has expired");

                var tempPasswordIsValid = requestingUser.TempCredentialsMatch(username, statefulProcessId);

                Guard.Against(() => !tempPasswordIsValid, "Request failed", "Credentials provided for password reset invalid");

                return requestingUser;
            }

            void CreateNewSecurityToken(SecureHmacHash hash, Guid userId, out SecurityToken newToken)
            {
                newToken = SecurityToken.Create(userId, hash.HexHash, DateTime.UtcNow, new TimeSpan(0, 15, 0), true);
            }

            void CreateNewPasswordHash(out SecureHmacHash newHash)
            {
                newHash = SecureHmacHash.CreateFrom(newPassword);
            }

            void DetermineChange(User user, SecureHmacHash newHash, SecurityToken newToken, out Action<User> change)
            {
                change = u =>
                    {
                    user.PasswordDetails.PasswordHash = newHash.HexHash;
                    user.PasswordDetails.HexSalt = newHash.HexSalt;
                    user.PasswordDetails.HashIterations = newHash.IterationsUsed;
                    user.PasswordDetails.PasswordResetStatefulProcessId = null;
                    user.PasswordDetails.PasswordResetTokenExpiry = null;
                    user.History.PasswordLastChanged = DateTime.UtcNow;
                    user.ActiveSecurityTokens = new List<SecurityToken>
                    {
                        newToken
                    };
                    user.History.InvalidLoginCount = 0;
                    };
            }
        }

        public async Task RevokeAllAuthTokens(Guid userId)
        {
            await DataStore.UpdateById(userId, u => u.ActiveSecurityTokens.Clear());
        }

        public async Task RevokeAuthToken(Guid userId, string authToken)
        {
            {
                var token = await Validate();

                await DataStore.UpdateById(userId, u => u.ActiveSecurityTokens.Remove(token));
            }

            async Task<SecurityToken> Validate()
            {
                var token = SecurityToken.DecryptToken(authToken);

                var user = await DataStoreRead.ReadActiveById<User>(userId);
                
                Guard.Against(() => !user.HasSecurityToken(token), "Token does not exist.");

                return token;
            }
        }

        private User UserCreate(
            string email,
            string name,
            PasswordDetails passwordDetails,
            List<IApplicationPermission> permissions,
            string username,
            Guid? userId = null)
        {
            return new User
            {
                Email = email,
                FullName = name,
                PasswordDetails = passwordDetails,
                Permissions = permissions,
                UserName = username,
                id = userId ?? Guid.NewGuid() //TODO remove id from IUserWithPermissions
            };
        }
    }
}