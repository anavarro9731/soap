﻿namespace Soap.Pf.MsmqEndpointBase
{
    public class MsmqEndpointWindowsServiceSettings
    {
        public string Description { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public bool StartAutomatically { get; set; } = true;
    }
}