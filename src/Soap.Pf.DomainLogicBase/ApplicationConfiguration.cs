// TODO
//namespace Soap.Pf.DomainLogicBase
//{
//    using DataStore.Interfaces;
//    using Soap.If.Interfaces;

//    public class ApplicationConfiguration : IApplicationConfig
//    {
//        public ApplicationConfiguration(
//            string environmentName,
//            string applicationVersion,
//            IApiEndpointSettings apiEndpointSettings,
//            IDatabaseSettings databaseSettings,
//            INotificationServerSettings notificationServerSettings,
//            bool returnExplicitErrorMessages = false,
//            byte numberOfApiMessageRetries = 1,
//            string defaultExceptionMessage = "An error has occurred.",
//            string applicationName = null,
//            ISeqLoggingConfig seqLoggingSettings = null)
//        {
//            EnvironmentName = environmentName;
//            ApiEndpointSettings = apiEndpointSettings;
//            DatabaseSettings = databaseSettings;
//            NotificationServerSettings = notificationServerSettings;
//            NumberOfApiMessageRetries = numberOfApiMessageRetries;
//            DefaultExceptionMessage = defaultExceptionMessage;
//            ReturnExplicitErrorMessages = returnExplicitErrorMessages;
//            ApplicationName = applicationName;
//            SeqLoggingSettings = seqLoggingSettings;
//            ApplicationVersion = GetType().Assembly.GetName().Version.ToString(3);
//        }

//        public IApiEndpointSettings ApiEndpointSettings { get; }

//        public string ApplicationName { get; }

//        public string ApplicationVersion { get; }

//        public IDatabaseSettings DatabaseSettings { get; }

//        public string DefaultExceptionMessage { get; }

//        public string EnvironmentName { get; }

//        public INotificationServerSettings NotificationServerSettings { get; }

//        public byte NumberOfApiMessageRetries { get; set; }

//        public bool ReturnExplicitErrorMessages { get; }

//        public ISeqLoggingConfig SeqLoggingSettings { get; }

//        public class SeqLoggingConfig : ISeqLoggingConfig
//        {
//            public SeqLoggingConfig(string serverUrl, string apiKey)
//            {
//                ApiKey = apiKey;
//                ServerUrl = serverUrl;
//            }

//            public string ApiKey { get; }

//            public string ServerUrl { get; }
//        }
//    }
//}

