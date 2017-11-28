﻿namespace Soap.Interfaces
{
    public interface IApiEndpointSettings
    {
        string HttpEndpointUrl { get; }

        string MsmqEndpointAddress { get; }

        string MsmqEndpointHost { get; }

        string MsmqEndpointName { get; }
    }
}