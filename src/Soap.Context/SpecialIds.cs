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
        
        public static readonly Guid FailsEarlyInReplayThenCompletesRemainderOfUow = Guid.NewGuid();

        public static readonly Guid ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds = Guid.NewGuid();

        //* exceptions
        public static readonly Guid MessageThatDiesInExceptionHandlerFirstStage = Guid.NewGuid();

        public static readonly Guid MessageThatDiesInExceptionHandlerSecondStage = Guid.NewGuid();

        public static readonly Guid MessageDiesWhileSavingUnitOfWork = Guid.NewGuid();

        public static readonly Guid ProcessesDataButFailsBeforeMessagesRetriesSuccessfully = Guid.NewGuid();
        
        public static readonly Guid ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully = Guid.NewGuid();

        public static readonly Guid ProcessesSomeThenRollsBackSuccessfully = Guid.NewGuid();

        public static readonly Guid RollbackSkipsOverItemsDeletedSinceWeChangedThem = Guid.NewGuid();

        public static readonly Guid RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem = Guid.NewGuid();
        
        public static readonly Guid RollbackSkipsOverItemsDeletedSinceWeCreatedThem = Guid.NewGuid();
        

        public static readonly Guid RollbackSkipsOverItemsUpdatedSinceWeCreatedThem = Guid.NewGuid();
    }
}