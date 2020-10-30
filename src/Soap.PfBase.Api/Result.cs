﻿namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using Soap.Interfaces.Messages;

    public class Result
    {
        public bool Success;

        public Exception UnhandledError { get; set; }

        public List<ApiCommand> CommandsSent { get; set; }

        public List<ApiEvent> PublishedMessages { get; set; }
    }
}