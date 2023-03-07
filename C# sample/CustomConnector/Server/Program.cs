// ---------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using CustomConnector.Connector;
using CustomConnector.Server;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace CustomConnector
{
    /// <summary>
    /// Main program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Process entry
        /// </summary>
        public static void Main()
        {
            var server = new ConnectorServer();
            server.StartLogger();
            server.Start();
            WaitForShutdown();
            server.Stop();
        }

        /// <summary>
        /// [Optional]
        /// Default implementation does not have an exit condition.
        /// Update it to handle custom server exit event.
        /// </summary>
        private static void WaitForShutdown()
        {
            Thread.Sleep(Timeout.Infinite);
        }
    }
}