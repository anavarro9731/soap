namespace Sample.Tests
{
    using System;
    using DataStore.Models.PureFunctions.Extensions;
    using Sample.Constants;
    using Sample.Messages.Commands;
    using Soap.Interfaces.Messages;

    public partial class BaseTest
    {
        /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

        protected static class Ids
        {
         //* message is retried max times and thrown out as poison
         public static readonly Guid MessageThatDiesWhileSavingUnitOfWork = Guid.NewGuid();

         //* message dies after commiting uow and then retries everything successfully on the first retry attempt
         public static readonly Guid RetryHappyPath = Guid.NewGuid();

         //* message dies after commiting uow and then retries everything but fails on the last item so it rolls everything
         //back successfully
         public static readonly Guid RollbackHappyPath = Guid.NewGuid();

         /* message dies after committing uow and knows before starting the retry, which retries the message without
          skipping the existing uow. likely it just dies after the max retries as the thing stopping it is unlikely
          to ever be removed unless it is rolled back itself but i think this was just the easiest way to handle it
          the only other option would be to throw an error which would result in retries anyway so might as well give
          it a chance if it doesn't cost you anything. skipping the retries altogether would mean more complex code */
         public static readonly Guid GiveUpOnRetry = Guid.NewGuid();

         /* message dies after committing then the retry dies because an item to be updated has been changed or removed
          resulting in a rollback of the previously committed items (1x create and 1 x delete and 1x softdelete) */
         public static readonly Guid UpdateFailsOnRetry = Guid.NewGuid();

         /* message dies after committing then the retry dies because an item to be deleted item has been changed or removed
          resulting in a rollback of the previously committed items (1x create and 2 x update[1 x softdelete and regular update]) */
         public static readonly Guid DeleteFailsOnRetry = Guid.NewGuid();

         //* data complete bus messages not complete, so send the incomplete ones
         public static readonly Guid SentUnsentMessages = Guid.NewGuid();

         /* message fails while attempting to commit uncommitted data due to etag conflict which
          should then on the 3rd try result in a rollback of everything committed so far.  */
         public static readonly Guid RetryingUowFailsSoRollback = Guid.NewGuid();

         /* while attempting to rollback on the second retry we encounter items that have already been updated again since our
          update and so we should just skip over those while rolling back. IF THE ITEM  IN QUESTION  WAS CREATED AND THEN UPDATED IN THE 
          SAME UOW THEN THIS COULD RESULT IN INCONSISTENT DATA but seems best compromise for now, maybe some collapse of such
          requests into one create could help but this not going to be a frequent case */
         public static readonly Guid RollbackSkipsOverSubsequentUpdates = Guid.NewGuid();

         /* while attempting to rollback on the second retry we encounter items that have already been updated again since we
         created them and so we should just skip over those while rolling back. THIS WILL RESULT IN INCONSISTENT DATA  but its
         the best compromise */
         public static readonly Guid RollbackSkipsOverSubsequentUpdateToCreatedAggregate = Guid.NewGuid();

         /* while attempting to rollback on the second retry we encounter items that have been hard deleted since we
          updated them so we should just skip over those while rolling back */
         public static readonly Guid RollbackSkipsoVerItemsDeletedSinceWeChangedThem = Guid.NewGuid();

         /* while attempting to rollback on the second retry we encounter items that have been hard deleted since we
          created them so we should just skip over those while rolling back */
         public static readonly Guid RollbackSkipsoVerItemsDeletedSinceWeCreatedThem = Guid.NewGuid();



         //* exceptions
         public static readonly Guid MessageThatDiesInExceptionHandlerFirstStage = Guid.NewGuid();

         public static readonly Guid MessageThatDiesInExceptionHandlerSecondStage = Guid.NewGuid();
        }
    }
}