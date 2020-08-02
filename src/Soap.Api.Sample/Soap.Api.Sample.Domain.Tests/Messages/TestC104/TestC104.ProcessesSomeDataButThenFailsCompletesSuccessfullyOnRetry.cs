namespace Sample.Tests.Messages
{
    using System.Threading.Tasks;
    using DataStore;
    using Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry : TestC104
    {
        public TestC104ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        //* message dies after commiting uow and then retries everything successfully on the first retry attempt
        public async void ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry()
        {
            //act
            var c104TestUnitOfWork =
                Commands.TestUnitOfWork(SpecialIds.ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1, beforeRunHook, c104TestUnitOfWork.Headers.GetMessageId()); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);

            async Task beforeRunHook(DataStore store, int run)
            {
                await FixSolosBrokenEtagSoItSucceeds();

                async Task FixSolosBrokenEtagSoItSucceeds()
                {
                    if (run == 2)
                    {
                        await store.UpdateById<User>(
                            Ids.HanSolo,
                            luke => { luke.Etag = "123456"; },
                            side => side.DisableOptimisticConcurrency());
                        await store.CommitChanges();
                    }
                }
            }
        }
    }
}