﻿namespace Soap.Interfaces.Messages
{
    using System.Reflection;

    public static class SoapInterfacesMessages
    {
        public static Assembly GetAssembly => typeof(SoapInterfacesMessages).GetTypeInfo().Assembly;
    }
}