﻿namespace Sample.Tests
{
    using System;
    using DataStore.Models.PureFunctions.Extensions;
    using Sample.Constants;
    using Sample.Messages.Commands;
    using Soap.Interfaces.Messages;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

    public static class Commands
    {
        public static C102GetServiceState GetServiceState => new C102GetServiceState();

        public static C100Ping Ping =>
            new C100Ping
            {
                PingedAt = DateTime.UtcNow
            };

        public static C104TestUnitOfWork 
            TestUnitOfWork(Guid? messageId = null) =>
            new C104TestUnitOfWork
            {
                HansSoloNewName = "Harrison Ford"
            }.Op(x => x.Headers.SetMessageId(messageId ?? Ids.C104CompletesSuccessfully));

        public static C101UpgradeTheDatabase UpgradeTheDatabaseToV1 => new C101UpgradeTheDatabase(ReleaseVersions.V1);

        public static C101UpgradeTheDatabase UpgradeTheDatabaseToV2 => new C101UpgradeTheDatabase(ReleaseVersions.V2);
    }
}