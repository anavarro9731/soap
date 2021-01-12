namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P208ReturnTestDataById : Process, IBeginProcess<C110v1_GetTestDataById>
    {
        public Func<C110v1_GetTestDataById, Task> BeginProcess =>
            async msg =>
                {
                var testData = await this.Get<TestDataQueries>().Call(x => x.GetTestDataById)(msg.C110_TestDataId.Value);

                var response = new E102v1_GotTestDatum
                {
                    E102_TestData = new E102v1_GotTestDatum.TestData
                    {
                        E102_Boolean = testData.Boolean,
                        E102_Decimal = testData.Decimal,
                        E102_File = testData.File,
                        E102_Guid = testData.Guid,
                        E102_Image = testData.Image,
                        E102_Long = testData.Long,
                        E102_String = testData.String,
                        E102_BooleanOptional = testData.BooleanOptional,
                        E102_CustomObject = new E102v1_GotTestDatum.TestData.Address
                        {
                            E102_House = testData.CustomObject.House,
                            E102_Town = testData.CustomObject.Town,
                            E102_PostCode = testData.CustomObject.Town
                        },
                        E102_DateTime = testData.DateTime,
                        E102_DecimalOptional = testData.DecimalOptional,
                        E102_FileOptional = testData.FileOptional,
                        E102_GuidOptional = testData.GuidOptional,
                        E102_ImageOptional = testData.ImageOptional,
                        E102_LongOptional = testData.LongOptional,
                        E102_StringOptional = testData.StringOptional,
                        E102_DateTimeOptional = testData.DateTimeOptional,
                        E102_PostCodesMulti = testData.PostCodesMulti,
                        E102_PostCodesSingle = testData.PostCodesSingle,
                        E102_StringOptionalMultiline = testData.StringOptionalMultiline,
                        E102_PostCodesMultiOptional = testData.PostCodesMultiOptional,
                        E102_PostCodesSingleOptional = testData.PostCodesSingleOptional
                    }
                };

                await Bus.Publish(response, new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                };
    }
}
