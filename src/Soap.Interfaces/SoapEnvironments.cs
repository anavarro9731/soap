// -----------------------------------------------------------------------
// <copyright file="$FILENAME$" company="$COMPANYNAME$">
// $COPYRIGHT$
// </copyright>
// <summary>
// $SUMMARY$
// </summary>

namespace Soap.MessagePipeline.Context
{
    using Soap.Utility.Objects.Blended;

    public class SoapEnvironments : Enumeration<SoapEnvironments>
    {
        public static SoapEnvironments Development = Create(nameof(Development), "DEV");

        public static SoapEnvironments InMemory = Create(nameof(InMemory), "INM");

        public static SoapEnvironments Live = Create(nameof(Live), "LIVE");

        public static SoapEnvironments Test = Create(nameof(Test), "TEST");
    }
}