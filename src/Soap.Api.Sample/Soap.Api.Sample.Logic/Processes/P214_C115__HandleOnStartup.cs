
    namespace Soap.Api.Sample.Logic.Processes
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using DataStore.Interfaces.LowLevel.Permissions;
        using Soap.Api.Sample.Messages.Commands;
        using Soap.Api.Sample.Models.Aggregates;
        using Soap.Context.Context;
        using Soap.Interfaces;
        using Soap.PfBase.Logic.ProcessesAndOperations;
    
        public class P214_C115__HandleOnStartup : Process, IBeginProcess<C115v1_OnStartup>
        {
            public Func<C115v1_OnStartup, Task> BeginProcess =>
                async message =>
                    {
                    {
                        await Task.CompletedTask;
                        /* SUGGESTION
                         On a new service if you set the AuthLevel to AutoApiAndDb
                         This will prevent you from processing any messages that access 
                         the database unless you have the appropriate role scopes and data scopes.
                         Both of these things have to be done in code. So we recommend to
                         setup a master user with root access and any initial data in this handler
                         which is in newer version (or can be) easily associated to the HealthCheck
                         as a StartupCommand like C101_UpgradeTheDatabase.
                         
                         You can setup a user with wildcard access to the data using the sample code below.
                                             
                         When you access data in the store, and you have AutoApiAndDb enabled 
                         for the query to succeed one of four things must be true.
                        
                         1. The users role scopes and data scopes must intersect by id.
                         2. The users role scope must be the special wildcard combination of AggregateReference(Guid.Empty, "Any", "*")
                         3. The aggregate being access must be marked with the [BypassSecurity] attribute
                         4. The call is made with the options.ByPassSecurity() option e.g. store.Read<X>(options => options.ByPassSecurity);
                           
                         */
                        // var tenant = new Tenant()
                        // {
                        //     id = Guid.Parse("9F24508C-D788-4D64-87A3-F1477EBF13D9"),
                        //     Name = "Test Tenant"
                        // };
                        //
                        // var dataStore = ContextWithMessageLogEntry.Current.DataStore;
                        // if (await dataStore.ReadById<Tenant>(tenant.id) == null)
                        // {
                        //     await dataStore.Create(tenant);
                        // }
                        //
                        // var rootUserProfile = new UserProfile()
                        // {
                        //     id = Guid.Parse("F941DB4F-98E5-431F-A86B-3DBA3170B20E"),
                        //     FirstName = "Sys",
                        //     LastName = "Admin",
                        //     Email = "sysadmin@mycompany.com",
                        //     UserName = "sysadmin",
                        //     TenantId = tenant.id
                        // };
                        //
                        // if (await dataStore.ReadById<UserProfile>(rootUserProfile.id) == null)
                        // {
                        //     /* creating the user in datastore and then in auth0 can be done separately but apart from this
                        //      exceptional case would usually be best wrapped into a process and always executed together */
                        //     await dataStore.Create(rootUserProfile);
                        //     /* make sure you have setup Auth0 in your config, and that you have a default Auth0 database connection for this
                        //     user to be added to specified as Auth0NewUserConnection = "{connection-name}"; */
                        //     var userId = await IDAAM.AddUser(new IIdaamProvider.AddUserArgs(rootUserProfile, "change-me-im-too-weak", true));
                        //     await IDAAM.AddRoleToUser(userId, Roles.Admin, new AggregateReference(Guid.Empty, "Any", "*"));    
                        // }
                    }
                    };
        }
    }
    
