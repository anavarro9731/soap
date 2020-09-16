namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Serilog;
    using Soap.Bus;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public interface ServiceBusManagementFunctions
    {
        static async Task CreateSubscriptionsIfNotExist(
            IServiceBusNamespace serviceBusNamespace,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Assembly messagesAssembly,
            Func<string, ValueTask> stream)
        {
            {
                var eventNames = mapMessagesToFunctions.Events.Select(e => e.FullName.ToLower());

                await stream($"Found {eventNames.Count()} Topic handled by this service, checking subscriptions...");

                foreach (var eventName in eventNames)
                    if ((await serviceBusNamespace.Topics.ListAsync()).Any(
                        x => string.Equals(x.Name, eventName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var queueName = messagesAssembly.GetName().Name.ToLower();

                        var topic = await serviceBusNamespace.Topics.GetByNameAsync(eventName);

                        if ((await topic.Subscriptions.ListAsync()).All(
                            x => !string.Equals(x.Name, queueName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            await stream($"-> Creating Subscription {queueName} for topic {topic.Name}");
                            await topic.Subscriptions.Define(queueName).CreateAsync();
                        }
                        else
                        {
                            await stream($"-> Subscription for topic {topic.Name} already exists");
                        }

                        await SetAutoForward(
                            serviceBusNamespace.ResourceGroupName,
                            serviceBusNamespace.Name,
                            topic.Name,
                            queueName,
                            queueName,
                            stream,
                            logger);
                    }
                    else
                    {
                        var messageTemplate = $"-> Cannot subscribe to topic {eventName} which does not appear to exist";
                        logger.Error(messageTemplate);
                        await stream(messageTemplate);
                    }
            }

            static async Task SetAutoForward(
                string resourceGroup,
                string busNamespace,
                string topicName,
                string subscriptionName,
                string forwardQueue,
                Func<string, ValueTask> stream,
                ILogger logger)
            {
                using var powerShell = PowerShell.Create();

                var command =
                    $"az servicebus topic subscription update --resource-group {resourceGroup} --namespace-name {busNamespace} --topic-name {topicName} --name {subscriptionName} --forward-to {forwardQueue}";
                await stream($"Forwarding event subscription {topicName} to service input queue");
                await stream(command);
                powerShell.AddScript(command);

                var PSOutput = powerShell.Invoke();

                foreach (var outputItem in PSOutput)
                    //* if null object was dumped to the pipeline during the script then a null object may be present here
                    if (outputItem != null)
                    {
                        await stream($"stdout> {outputItem}");
                    }

                // check the other output streams (for example, the error stream)
                if (powerShell.Streams.Error.Count > 0)
                {
                    // error records were written to the error stream.
                    foreach (var errorRecord in powerShell.Streams.Error)
                    {
                        await stream($"Error setting forward queue: <{errorRecord.ErrorDetails?.Message}>");
                        logger.Error(errorRecord.Exception, "Error setting forward queue");
                    }

                    foreach (var warningRecord in powerShell.Streams.Warning)
                        await stream($"Warning setting forward queue {warningRecord.Message}");
                }
            }
        }

        static async Task CreateTopicsIfNotExist(
            IServiceBusNamespace serviceBusNamespace,
            Assembly messagesAssembly,
            Func<string, ValueTask> stream)
        {
            IEnumerable<ApiMessage> events = messagesAssembly.GetTypes()
                                                             .Where(t => t.InheritsOrImplements(typeof(ApiEvent)))
                                                             .Select(x => Activator.CreateInstance(x) as ApiEvent)
                                                             .ToList();

            await stream($"Found {events.Count()} Topics published by this service ...");

            var topics = (await serviceBusNamespace.Topics.ListAsync()).ToList();

            foreach (var @event in events)
            {
                var topicName = @event.GetType().FullName.ToLower();

                if (topics.All(t => !string.Equals(t.Name, topicName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    await stream($"-> Creating Topic {topicName}");
                    await serviceBusNamespace.Topics.Define(topicName)
                                             .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                             .CreateAsync();
                }
                else
                {
                    await stream($"-> Topic {topicName} already created");
                }
            }
        }

        static async Task<IServiceBusNamespace> GetNamespace(
            AzureBus.Settings busSettings,
            IAzure azure,
            Func<string, ValueTask> stream)
        {
            await stream($"Attaching to namespace {busSettings.BusNamespace}");

            var serviceBusNamespace = await azure.ServiceBusNamespaces.GetByResourceGroupAsync(
                                          busSettings.ResourceGroup,
                                          busSettings.BusNamespace);

            return serviceBusNamespace;
        }
    }
}