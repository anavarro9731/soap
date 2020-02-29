using System;
using System.Collections.Generic;
using System.Text;

namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    public interface IMapMessagesToFunctions
    {
        IMessageFunctions Map(ApiMessage message);
    }
}
