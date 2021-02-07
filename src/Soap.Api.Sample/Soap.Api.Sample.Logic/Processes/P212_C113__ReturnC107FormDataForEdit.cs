namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P212_C113__ReturnC107FormDataForEdit : Process, IBeginProcess<C113v1_GetC107FormDataForEdit>
    {
        /* there is always a security concern to consider when writing processes that return form data
        if the form data is not default data, and that is whether the user has access to that data.
        this is why there should be separate commands for retrieving the default state of a form and
        for a populated state. you could share the same response event and just populate it differntly
        that is ok. there is also the matter of whether the user has permissions to send the subsequent
        command posed by the form presented but that is handled through the normal security process which 
        could result in the scenario where you could request a form with default values that you do not
        have the right to send. unless the default values are sensitive data it shouldnt be an issue */

        private readonly List<Enumeration> postCodes = new List<Enumeration>
        {
            new Enumeration("hr20dn", "HR2 0DN"),
            new Enumeration("al55ng", "AL55NG"),
            new Enumeration("ox29ju", "OX2 9JU")
        };

        public Func<C113v1_GetC107FormDataForEdit, Task> BeginProcess =>
            async message =>
                {
                var testData = await this.Get<TestDataQueries>().Call(x => x.GetTestDataById)(message.C113_TestDataId.Value);

                var formData = new C107v1_CreateOrUpdateTestDataTypes
                {
                    C107_Boolean = testData.Boolean,
                    C107_Decimal = testData.Decimal,
                    C107_File = testData.File,
                    C107_Guid = testData.Guid,
                    C107_Image = testData.Image,
                    C107_Long = testData.Long,
                    C107_String = testData.String,
                    C107_BooleanOptional = testData.BooleanOptional,
                    C107_CustomObject = new C107v1_CreateOrUpdateTestDataTypes.Address
                    {
                        C107_House = testData.CustomObject?.House,
                        C107_Town = testData.CustomObject?.Town,
                        C107_PostCode = testData.CustomObject?.PostCode
                    },
                    C107_DateTime = testData.DateTime,
                    C107_DecimalOptional = testData.DecimalOptional,
                    C107_FileOptional = testData.FileOptional,
                    C107_GuidOptional = testData.GuidOptional,
                    C107_ImageOptional = testData.ImageOptional,
                    C107_LongOptional = testData.LongOptional,
                    C107_StringOptional = testData.StringOptional,
                    C107_DateTimeOptional = testData.DateTimeOptional,
                    C107_StringOptionalMultiline = testData.StringOptionalMultiline,
                    C107_PostCodesSingle = new EnumerationAndFlags(
                        allEnumerations: this.postCodes,
                        allowMultipleSelections: false).AddFlagIfItExistsInAllEnumerations(testData.PostCodesSingleKey),
                    C107_PostCodesSingleOptional = new EnumerationAndFlags(
                        allEnumerations: this.postCodes,
                        allowMultipleSelections: false).AddFlagIfItExistsInAllEnumerations(testData.PostCodesSingleOptionalKey),
                    C107_PostCodesMulti =
                        new EnumerationAndFlags(allEnumerations: this.postCodes).AddFlagsIfTheyExistInAllEnumerations(
                            testData.PostCodesMultiKeys),
                    C107_PostCodesMultiOptional =
                        new EnumerationAndFlags(allEnumerations: this.postCodes).AddFlagsIfTheyExistInAllEnumerations(
                            testData.PostCodesMultiOptionalKeys)
                };

                await PublishFormDataEvent(new E103v1_GotC107FormData(), formData);
                };
    }

    public static class FlagExts
    {
        public static EnumerationAndFlags AddFlagIfItExistsInAllEnumerations(this EnumerationAndFlags flags, string key)
        {
            if (flags.AllEnumerations.Any(f => f.Key == key))
            {
                flags.AddFlag(flags.AllEnumerations.Single(x => x.Key == key));
            }

            return flags;
        }

        public static EnumerationAndFlags AddFlagsIfTheyExistInAllEnumerations(this EnumerationAndFlags flags, List<string> keys)
        {
            keys ??= new List<string>();
            var knownKeys = keys.Where(key => flags.AllEnumerations.Any(f => f.Key == key)).ToList();
            foreach (var key in knownKeys) flags.AddFlag(flags.AllEnumerations.Single(x => x.Key == key));

            return flags;
        }
    }
}
