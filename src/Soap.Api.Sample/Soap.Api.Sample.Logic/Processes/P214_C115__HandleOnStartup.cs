
    namespace Soap.Api.Sample.Logic.Processes
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using DataStore.Interfaces.LowLevel.Permissions;
        using Soap.Api.Sample.Afs.Security;
        using Soap.Api.Sample.Messages.Commands;
        using Soap.Api.Sample.Models.Aggregates;
        using Soap.Interfaces;
        using Soap.PfBase.Logic.ProcessesAndOperations;
    
        public class P214_C115__HandleOnStartup : Process, IBeginProcess<C115v1_OnStartup>
        {
            public Func<C115v1_OnStartup, Task> BeginProcess =>
                async message =>
                    {
                    {
                        
                    }
                    };
        }
    }
    
