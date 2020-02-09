namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IMapErrorCodesFromDomainToMessageErrorCodes
    {
        Dictionary<ErrorCode, ErrorCode> DefineMapper();
    }
}