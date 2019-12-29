﻿namespace Soap.If.Utility.PureFunctions
{
    using System;
    using System.Reflection;

    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
            CallingAssembly = Assembly.GetCallingAssembly().GetName().Name;
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal DomainException()
        {
        }

        public string CallingAssembly { get; set; }
    }
}