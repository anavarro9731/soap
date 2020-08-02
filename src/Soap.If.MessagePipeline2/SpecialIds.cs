namespace Soap.MessagePipeline
{
    using System;

    /// <summary>
    ///     Ids used to identify special message behaviour in testing
    /// </summary>
    public static class SpecialIds
    {
        public static readonly Guid ConsideredAsRolledBackWhenFirstItemFails = Guid.NewGuid();

        public static readonly Guid FailsDuringRollbackFinishesRollbackOnNextRetry = Guid.NewGuid();

        public static readonly Guid FailsToProcessAnyButThenRetriesSuccessfully = Guid.NewGuid();

        public static readonly Guid ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry = Guid.NewGuid();
        
        public static readonly Guid FailsEarlyTestsCreateUpdateDeleteUowCompletion = Guid.NewGuid();

        //* exceptions
        public static readonly Guid MessageThatDiesInExceptionHandlerFirstStage = Guid.NewGuid();

        public static readonly Guid MessageThatDiesInExceptionHandlerSecondStage = Guid.NewGuid();

        public static readonly Guid MessageDiesWhileSavingUnitOfWork = Guid.NewGuid();

        public static readonly Guid ProcessesDataButFailsBeforeMessagesRetriesSuccessfully = Guid.NewGuid();

        public static readonly Guid ProcessesSomeThenRollsBackSuccessfully = Guid.NewGuid();

        /* while attempting to rollback on the second retry we encounter items that have been hard deleted since we
         updated them so we should just skip over those while rolling back */
        public static readonly Guid RollbackSkipsOverItemsDeletedSinceWeChangedThem = Guid.NewGuid();

        /* while attempting to rollback on the second retry we encounter items that have been hard deleted since we
         created them so we should just skip over those while rolling back */
        public static readonly Guid RollbackSkipsOverItemsDeletedSinceWeCreatedThem = Guid.NewGuid();

        /* while attempting to rollback we encounter items that have already been updated again since our
         update and so we should just skip over those while rolling back. IF THE ITEM  IN QUESTION  WAS CREATED AND THEN UPDATED IN THE 
         SAME UOW THEN THIS COULD RESULT IN INCONSISTENT DATA but seems best compromise for now, maybe some collapse of such
         requests into one create could help but this not going to be a frequent case */
        public static readonly Guid RollbackSkipsOverSubsequentUpdates = Guid.NewGuid();

        /* while attempting to rollback on the second retry we encounter items that have already been updated again since we
        created them and so we should just skip over those while rolling back. THIS WILL RESULT IN INCONSISTENT DATA  but its
        the best compromise */
        public static readonly Guid RollbackSkipsOverSubsequentUpdateToCreatedAggregate = Guid.NewGuid();
    }
}