using System;

namespace Soap.Context.UnitOfWork
{
    using CircuitBoard;
    using Soap.Utility;

    public class UnitOfWorkErrorCodes : ErrorCode
    {
        /* Error Codes only need to be mapped if there is front-end logic that might depend on them
     otherwise the default error handling logic will do the job of returning the error message but without a specific code. */

        public static readonly ErrorCode UnitOfWorkFailedUnitOfWorkRolledBack = Create(
            Guid.Parse("36312a82-ca04-4b09-978f-5bb9e2809c2d"),
            "Unit of work was rolled back successfully and cannot be processed again");
    }
}
