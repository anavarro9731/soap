namespace Soap.Interfaces
{
    public interface IApplicationConfig : IBootstrapVariables
    {
        string FunctionAppHostUrlWithTrailingSlash { get; set; }

        string FunctionAppHostName { get; set; }

        string CorsOrigin { get; set; }
    }
}