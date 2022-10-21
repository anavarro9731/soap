namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P216_C116__HandleTruncateMessageLog : Process, IBeginProcess<C116v1_TruncateMessageLog>
    {
        public Func<C116v1_TruncateMessageLog, Task> BeginProcess =>
            async message =>
                {
                var dataStore = ContextWithMessageLogEntry.Current.DataStore;
                if (message.C116_From.HasValue && message.C116_To.HasValue)
                {
                    await dataStore.DeleteWhere<MessageLogEntry>(
                        x => x.Created >= message.C116_From.Value && x.Created <= message.C116_To.Value,
                        options => options.Permanently());
                }
                else if (message.C116_From.HasValue)
                {
                    await dataStore.DeleteWhere<MessageLogEntry>(
                        x => x.Created >= message.C116_From.Value,
                        options => options.Permanently());
                }
                else if (message.C116_To.HasValue)
                {
                    await dataStore.DeleteWhere<MessageLogEntry>(
                        x => x.Created <= message.C116_To.Value,
                        options => options.Permanently());
                }
                else
                {
                    var defaultWindowThreshold = DateTime.UtcNow.AddDays(-7);

                    //* check message count
                    var count = await dataStore.WithoutEventReplay.Count<MessageLogEntry>(
                                    x => x.Created <= defaultWindowThreshold);
                    if (count > 1000)
                    {
                        var ct = new ContinuationToken();
                        var oldestDatedLogEntryEnumerable =
                            await dataStore.WithoutEventReplay.Read<MessageLogEntry>(
                                null,
                                options => options.OrderBy(x => x.CreatedAsMillisecondsEpochTime).Take(1, ref ct));
                        var oldestLogEntry = oldestDatedLogEntryEnumerable.Single();

                        if (oldestLogEntry.Created <= defaultWindowThreshold)
                            //* batch response by day
                        {
                            for (var dt = oldestLogEntry.Created.Date; dt <= defaultWindowThreshold.Date; dt = dt.AddDays(1))
                                await Bus.Send(
                                    new C116v1_TruncateMessageLog
                                    {
                                        C116_From = dt.ToUniversalTime(),
                                        C116_To = dt.AddDays(1).ToUniversalTime()
                                    },
                                    true);
                        }
                    }
                    else
                    {
                        await dataStore.DeleteWhere<MessageLogEntry>(
                            x => x.Created <= defaultWindowThreshold,
                            options => options.Permanently());
                    }
                }
                };
    }
}