namespace Palmtree.Api.Sso.Domain.Logic.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.EndpointInfrastructure;

    public class UserOperations : Operations<User>
    {
        public async Task<User> AddDefaultUser()
        {
            {
                DetermineChange(out User user);

                await DataStore.Create(user);

                return user;
            }

            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(HardCodedMasterData.RootUser.Password);

                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = User.Create(
                    HardCodedMasterData.RootUser.EmailAddress,
                    HardCodedMasterData.RootUser.FullName,
                    passwordDetails,
                    null,
                    HardCodedMasterData.RootUser.UserName,
                    HardCodedMasterData.RootUser.UserId);

                user.Status.AddState(User.UserStates.EmailConfirmed);
            }
        }

        public async Task<User> AddFullyRegisteredUser(RegisterUser command)
        {
            {
                DetermineChange(out User user);

                await DataStore.Create(user);

                return user;
            }

            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(command.Password);
                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = User.Create(command.Email, command.Name, passwordDetails, null, command.Email, command.UserId);
                user.Status.AddState(User.UserStates.EmailConfirmed);
            }
        }

        public async Task<User> AddPartiallyRegisteredUser(RegisterUser command)
        {
            {
                DetermineChange(out User user);

                await DataStore.Create(user);

                return user;
            }

            void DetermineChange(out User user)
            {
                var passwordHash = SecureHmacHash.CreateFrom(command.Password);
                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                user = User.Create(command.Email, command.Name, passwordDetails, null, command.Email);
            }
        }

        public async Task AddThingToUser(Thing thing, Guid userId)
        {
            {
                DetermineChange(out Action<User> change, thing.id);

                await DataStore.UpdateById(userId, change);
            }

            void DetermineChange(out Action<User> change, Guid thingId)
            {
                change = u => u.ThingIds.Add(thingId);
            }
        }

        public async Task<ResetPasswordFromEmail.ClientSecurityContext> AuthenticateUser(AuthenticateUser command)
        {
            ResetPasswordFromEmail.ClientSecurityContext response = null;
            {
                var user = await FindUserWithMatchingUsername();

                Validate(user);

                DetermineChange(user, out Action<User> change);

                await DataStore.UpdateById(user.id, change, true);

                return response;
            }

            void Validate(User user)
            {
                new AuthenticateUserValidator().ValidateAndThrow(command);

                Guard.Against(() => user == null, "Login Failed. 01", "User does not exist.");

                Guard.Against(() => user.Status.HasState(User.UserStates.AccountDisabled), "Your account is disabled please contact support");

                Guard.Against(
                    user.UserAccountIsLocked,
                    "Your have already entered an incorrect password too many times. Your account is now locked. Please try again in five minutes.");

                Guard.Against(user.UserName != command.Credentials.Username, "These credentials are for a different user. Check code logic.");
            }

            async Task<User> FindUserWithMatchingUsername()
            {
                var matchingUsers = await DataStoreRead.ReadActive<User>(u => u.UserName == command.Credentials.Username);

                return matchingUsers.SingleOrDefault();
            }

            void DetermineChange(User user, out Action<User> change)
            {
                if (user.NormalCredentialsMatch(Credentials.Create(command.Credentials.Username, command.Credentials.Password)))
                {
                    var securityToken = SecurityToken.Create(user.id, user.PasswordDetails.PasswordHash, DateTime.UtcNow, new TimeSpan(0, 15, 0), true);

                    change = u => Login(u, securityToken);

                    response = Messages.Commands.ResetPasswordFromEmail.ClientSecurityContext.Create(securityToken, user);
                }
                else
                {
                    change = u => IncrementInvalidLoginData(u);

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
                DetermineChange(out Action<User> change);

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

        public async Task RequestPasswordReset(RequestPasswordReset requestPasswordReset, Guid passwordResetStatefulProcessId)
        {
            {
                var user = (await DataStoreRead.ReadActive<User>(u => u.Email == requestPasswordReset.Email)).SingleOrDefault();

                Validate(user);

                DetermineChange(out Action<User> change);

                await DataStore.UpdateById(user.id, change, true);

            }

            void Validate(User user)
            {
                new RequestPasswordResetValidator().ValidateAndThrow(requestPasswordReset);

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

        public async Task<ResetPasswordFromEmail.ClientSecurityContext> ResetPasswordFromEmail(ResetPasswordFromEmail command)
        {
            {
                var user = await Validate();

                CreateNewPasswordHash(out SecureHmacHash newHash);

                CreateNewSecurityToken(newHash, user.id, out SecurityToken newToken);

                DetermineChange(user, newHash, newToken, out Action<User> change);

                await DataStore.UpdateById(user.id, change, true);

                return Messages.Commands.ResetPasswordFromEmail.ClientSecurityContext.Create(newToken, user);
            }

            async Task<User> Validate()
            {
                new ResetPasswordFromEmailValidator().ValidateAndThrow(command);

                var requestingUser = (await DataStoreRead.Read<User>(user => user.UserName == command.Username)).SingleOrDefault();

                Guard.Against(() => requestingUser == null, "Request failed", "User does not exist");

                Guard.Against(() => requestingUser.AccountIsDisabled(), "Account is Disabled");

                Guard.Against(() => !requestingUser.Active, "Account not found", "Account is deleted");

                Guard.Against(() => requestingUser.PasswordResetRequestHasExpired(), "Temporary Password has expired");

                var tempPasswordIsValid = requestingUser.TempCredentialsMatch(command.Username, command.StatefulProcessId.Value);

                Guard.Against(() => !tempPasswordIsValid, "Request failed", "Credentials provided for password reset invalid");

                return requestingUser;
            }

            void CreateNewSecurityToken(SecureHmacHash hash, Guid userId, out SecurityToken newToken)
            {
                newToken = SecurityToken.Create(userId, hash.HexHash, DateTime.UtcNow, new TimeSpan(0, 15, 0), true);
            }

            void CreateNewPasswordHash(out SecureHmacHash newHash)
            {
                newHash = SecureHmacHash.CreateFrom(command.NewPassword);
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

        public async Task RevokeAllAuthTokens(RevokeAllAuthTokens command, ApiMessageMeta meta)
        {
            await DataStore.UpdateById(meta.RequestedBy.id, u => u.ActiveSecurityTokens.Clear());
        }

        public async Task RevokeAuthToken(RevokeAuthToken command, ApiMessageMeta meta)
        {
            {
                Validate(out SecurityToken token);

                await DataStore.UpdateById(meta.RequestedBy.id, u => u.ActiveSecurityTokens.Remove(token));
            }

            void Validate(out SecurityToken tokenOut)
            {
                var token = SecurityToken.DecryptToken(command.AuthToken);

                Guard.Against(() => !(meta.RequestedBy as User).HasSecurityToken(token), "Token does not exist.");

                tokenOut = token;
            }
        }
    }
}