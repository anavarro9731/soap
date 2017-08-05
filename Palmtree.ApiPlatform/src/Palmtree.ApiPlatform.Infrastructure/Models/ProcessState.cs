namespace Palmtree.ApiPlatform.Infrastructure.Models
{
    using System.Dynamic;
    using DataStore.Interfaces.LowLevel;
    using Palmtree.ApiPlatform.Utility;

    /// <summary>
    ///     stores the data for a process instance
    /// </summary>
    public class ProcessState : Aggregate
    {
        public FlaggedState Flags { get; set; }

        public ExpandoObject References { get; set; } = new ExpandoObject();
    }
}
