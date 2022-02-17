// ---------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate
{
    using System.Threading;
    using CustomConnectorTemplate.Server;
    using Serilog;

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
            InitializeLogger();
            Log.Information("Starting connector");
            var server = new ConnectorServer();
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

        /// <summary>
        /// Initializes serilog logger.
        /// Serilog is just an option. Feel free to use any of the logging frameworks.
        /// </summary>
        private static void InitializeLogger()
        {
            long logFileSizeLimitInBytes = 10 * 1000 * 1000; // 10 MB
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(@"Logs\ConnectorLog.log", fileSizeLimitBytes: logFileSizeLimitInBytes)
                .CreateLogger();
        }
    }
}
