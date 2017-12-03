namespace Soap.If.Interfaces.Messages
{
    public class ApiQuery : ApiMessage, IApiQuery
    {
    }

    public class ApiQuery<TReturnValue> : ApiQuery, IApiQuery<TReturnValue> where TReturnValue : class, new()
    {
        public TReturnValue ReturnValue { get; set; }
    }
}