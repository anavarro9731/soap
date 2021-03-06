namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus.Administration;
    using Serilog;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public interface ServiceBusManagementFunctions
    {
        static async Task CreateQueueIfNotExist(
            Assembly messagesAssembly,
            AzureBus.Settings busSettings,
            Func<string, ValueTask> stream)
        {
            var queueName = messagesAssembly.GetName().Name.ToLower() + "." + EnvVars.EnvironmentPartitionKey;

            var adminClient = new ServiceBusAdministrationClient(busSettings.BusConnectionString);

            await stream("Checking Queue...");

            if (!await adminClient.QueueExistsAsync(queueName))
            {
                await stream($"-> Queue {queueName} does not exist, creating it...");
                await adminClient.CreateQueueAsync(queueName);
            }
            else
            {
                await stream($"-> Queue {queueName} exists.");
            }
        }

        static async Task CreateSubscriptionsIfNotExist(
            AzureBus.Settings busSettings,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Assembly messagesAssembly,
            Func<string, ValueTask> stream)
        {
            {
                var eventNames = mapMessagesToFunctions.Events.Select(e => e.FullName);

                await stream($"Found {eventNames.Count()} Topic handled by this service, checking subscriptions...");

                var adminClient = new ServiceBusAdministrationClient(busSettings.BusConnectionString);

                foreach (var eventName in eventNames)
                {
                    var topicName = eventName.ToLowerInvariant();

                    if (await adminClient.TopicExistsAsync(topicName))
                    {
                        var queueName = messagesAssembly.GetName().Name.ToLower() + "." + EnvVars.EnvironmentPartitionKey;

                        Guard.Against(
                            queueName.Length > 50, //azure
                            $"Queue name plus EnvironmentPartitionKey {queueName} cannot exceed 50 characters");

                        if (await adminClient.SubscriptionExistsAsync(topicName, queueName) == false)
                        {
                            await stream($"-> Creating Subscription {queueName} for topic {topicName}");

                            await adminClient.CreateSubscriptionAsync(
                                new CreateSubscriptionOptions(topicName, queueName)
                                {
                                    ForwardTo = queueName,
                                    MaxDeliveryCount = 1
                                });

                            var filter = new CorrelationRuleFilter
                            {
                                ApplicationProperties =
                                {
                                    [nameof(EnvVars.EnvironmentPartitionKey)] = EnvVars.EnvironmentPartitionKey
                                }
                            };
                            var ruleOptions = new CreateRuleOptions(nameof(EnvVars.EnvironmentPartitionKey), filter);

                            await adminClient.CreateRuleAsync(topicName, queueName, ruleOptions);
                        }
                        else
                        {
                            await stream($"-> Subscription for topic {topicName} already exists");
                        }
                    }
                    else
                    {
                        var messageTemplate = $"-> Cannot subscribe to topic {eventName} which does not appear to exist";
                        logger.Error(messageTemplate);
                        await stream(messageTemplate);
                    }
                }
            }
        }

        static async Task CreateTopicsIfNotExist(
            AzureBus.Settings busSettings,
            Assembly messagesAssembly,
            Func<string, ValueTask> stream)
        {
            IEnumerable<ApiMessage> events = messagesAssembly.GetTypes()
                                                             .Where(t => t.InheritsOrImplements(typeof(ApiEvent)))
                                                             .Select(x => Activator.CreateInstance(x) as ApiEvent)
                                                             .ToList();

            await stream($"Found {events.Count()} Topics published by this service ...");

            var adminClient = new ServiceBusAdministrationClient(busSettings.BusConnectionString);

            foreach (var @event in events)
            {
                var topicName = @event.GetType().FullName.ToLowerInvariant();

                if (await adminClient.TopicExistsAsync(topicName) == false)
                {
                    await stream($"-> Creating Topic {topicName}");
                    await adminClient.CreateTopicAsync(
                        new CreateTopicOptions(topicName)
                        {
                            RequiresDuplicateDetection = true,
                            DuplicateDetectionHistoryTimeWindow = new TimeSpan(0, 1, 0, 0)
                        });
                }
                else
                {
                    await stream($"-> Topic {topicName} already created");
                }
            }
        }
    }
}
