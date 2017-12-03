namespace Soap.If.Interfaces.Messages
{
    public interface IApiQuery : IApiMessage
    {
    }

    public interface IApiQuery<T> : IApiQuery where T : class, new()
    {
        T ReturnValue { get; set; }
    }
}