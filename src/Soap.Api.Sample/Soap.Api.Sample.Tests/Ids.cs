namespace Soap.Api.Sample.Tests
{
    using System;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use equals on the property assignment and 
        be readonly as opposed to arrows because they are static and you will share message instances and have concurrency problems otherwise 
        e.g. static x => Guid.NewGuid() with an arrow will cause serious problems */

    public static partial class Ids
    {
        public static readonly string ApiIdOne = "some-auth0|Id";
    }
}
