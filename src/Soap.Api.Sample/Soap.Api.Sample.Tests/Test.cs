// -----------------------------------------------------------------------
// <copyright file="$FILENAME$" company="$COMPANYNAME$">
// $COPYRIGHT$
// </copyright>
// <summary>
// $SUMMARY$
// </summary>


namespace Sample.Tests
{
    using System;
    using Sample.Logic;
    using Xunit.Abstractions;

    public class Test : SoapMessageTest
    {
        protected Test(ITestOutputHelper output)
            : base(output, new MappingRegistration())
        {
        }
    }
}