namespace Soap.If.MessagePipeline.Models.Aggregates
{
    using System.Dynamic;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility;

    /// <summary>
    ///     stores the data for a process instance
    /// </summary>
    public class ProcessState : Aggregate
    {
        public Flags Flags { get; set; }

        public ExpandoObject References { get; set; } = new ExpandoObject();
    }
}