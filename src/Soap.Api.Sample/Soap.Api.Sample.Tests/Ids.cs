namespace Sample.Tests
{
    using System;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

    public static class Ids
    {
        public static readonly Guid C104CompletesSuccessfully = Guid.NewGuid();

        public static readonly Guid UserOne = Guid.NewGuid();
        
        public static readonly Guid PrincessLeia = Guid.Parse("715fb00d-f856-42b9-822e-fc0510c6fab5");
        
        public static readonly Guid LukeSkywalker = Guid.NewGuid();
        
        public static readonly Guid HanSolo = Guid.NewGuid();

        public static readonly Guid DarthVader = Guid.NewGuid();
    }
}