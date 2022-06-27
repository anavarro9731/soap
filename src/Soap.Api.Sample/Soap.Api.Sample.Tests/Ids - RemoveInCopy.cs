namespace Soap.Api.Sample.Tests
{
    using System;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use equals on the property assignment and 
        be readonly as opposed to arrows because
        they are static and you will share message instances and have concurrency problems otherwise */

    public  static partial class Ids
    {
        public static readonly Guid C104CompletesSuccessfully = Guid.NewGuid();

        public static readonly Guid PrincessLeia = Guid.Parse("715fb00d-f856-42b9-822e-fc0510c6fab5");
        
        public static readonly Guid LukeSkywalker = Guid.NewGuid();
        
        public static readonly Guid HanSolo = Guid.NewGuid();

        public static readonly Guid DarthVader = Guid.NewGuid();
    }
    
    public static class Ext {
        public static string ToIdaam(this Guid id) => $"idaam|{id.ToString()}";
    }
    
}
