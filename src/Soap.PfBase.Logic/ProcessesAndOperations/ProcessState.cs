namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System.Dynamic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Utility.Objects.Binary;

    /// <summary>
    ///     stores the data for a process instance
    /// </summary>
    [BypassSecurity("Because this is used by StatefulProcess' inside message Handlers rather than outside like everything else in the pipeline that accesses the database if AuthLevel=AutoApiAndDb you will be in the context of the user who won't have access to this by default")] 
    public class ProcessState : Aggregate
    {
        public EnumFlags EnumFlags { get; set; } = new EnumFlags();

        public ExpandoObject References { get; set; } = new ExpandoObject();
    }
}