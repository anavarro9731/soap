//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests
{
    using System;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

    public static partial class Commands
    {
        public static C104v1_TestUnitOfWork TestUnitOfWork(Guid? messageId = null) =>
            new C104v1_TestUnitOfWork
            {
                C104_HansSoloNewName = "Hairy-son Ford"
            }.Op(x =>
                {
                x.Headers.SetMessageId(messageId ?? Ids.C104CompletesSuccessfully);
                });
        
        public static C102v1_GetServiceState GetServiceState => new C102v1_GetServiceState();

        
    }
    
    
}
