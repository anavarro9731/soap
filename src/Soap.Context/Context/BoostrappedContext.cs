namespace Soap.Context.Context
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;

    public class BoostrappedContext
    {
        public readonly IdentityPermissions IdentityPermissions; //* allowed to be null, check callers, should only be used to set Meta

        public readonly IBootstrapVariables AppConfig;

        public readonly IBlobStorage BlobStorage;

        public readonly IBus Bus;

        public readonly DataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public readonly MapMessagesToFunctions MessageMapper;

        public readonly NotificationServer NotificationServer;

        private readonly Func<Task<IUserProfile>> GetUserProfileFromIdentityServer;

        public BoostrappedContext(
            IBootstrapVariables appConfig,
            DataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBus bus,
            NotificationServer notificationServer,
            BlobStorage blobStorage,
            MapMessagesToFunctions messageMapper,
            IdentityPermissions identityPermissions,
            Func<Task<IUserProfile>> getUserProfileFromIdentityServer)
        {
            this.MessageMapper = messageMapper;
            this.IdentityPermissions = identityPermissions;  //* allowed to be null
            this.GetUserProfileFromIdentityServer = getUserProfileFromIdentityServer;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.Bus = bus;
            this.NotificationServer = notificationServer;
            this.BlobStorage = blobStorage;
        }

        protected BoostrappedContext(BoostrappedContext c)
        {
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.Bus = c.Bus;
            this.NotificationServer = c.NotificationServer;
            this.MessageMapper = c.MessageMapper;
            this.BlobStorage = c.BlobStorage;
            this.IdentityPermissions = c.IdentityPermissions;
            this.GetUserProfileFromIdentityServer = c.GetUserProfileFromIdentityServer;
        }

        public async Task<TUserProfile> GetUserProfile<TUserProfile>() where TUserProfile : class, IUserProfile, IAggregate, new()
        {
            var userProfile = await this.GetUserProfileFromIdentityServer();

            var user = (await this.DataStore.Read<TUserProfile>(x => x.Auth0Id == userProfile.Auth0Id)).SingleOrDefault();

            if (user == null)
            {
                var newUser = new TUserProfile
                {
                    Auth0Id = userProfile.Auth0Id,
                    Email = userProfile.Email,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName
                };

                return (await this.DataStore.Create(newUser));
            }
            else
            {
             return (await this.DataStore.UpdateWhere<TUserProfile>(
                    u => u.Auth0Id == user.Auth0Id,
                    x =>
                        {
                        x.Email = userProfile.Email;
                        x.FirstName = userProfile.FirstName;
                        x.LastName = userProfile.LastName;
                        })).Single();
            }
        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextWithMessage Upgrade(
            this BoostrappedContext current,
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp) =>
            new ContextWithMessage(message, timeStamp, current);
    }
}
