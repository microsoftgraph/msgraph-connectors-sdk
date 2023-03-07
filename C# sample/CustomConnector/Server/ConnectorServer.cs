// ---------------------------------------------------------------------------
// <copyright file="ConnectorServer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using CustomConnector.Connector;
using Serilog;
using System;
using System.IO;

namespace CustomConnector.Server
{
    /// <summary>
    /// Class to host the grpc server
    /// </summary>
    public class ConnectorServer
    {
        /// <summary>
        /// TCP Port on which the server will listen for calls from Graph connectors platform
        /// This same port info need to be made part of connector manifest file
        /// Ensure that no other application is blocking the port or select a port number that is ensured to be free on production environment.
        /// Field is made accessible to outside of the class to be able to update it from a config file if needed
        /// </summary>
        public static int Port { get; set; } = 30303;

        /// <summary>
        /// [Optional]
        /// Enable this flag to encrypt in transit data between Graph connectors platform and connector
        /// A valid certificate and key files are needed to enable this flow. File paths of both certificate and
        ///     its key to be updated in CertificateFilePath and CertificateKeyFilePath respectively
        /// Field is made accessible to outside of the class to be able to update it from a config file if needed
        /// </summary>
        public static bool UseSslEncryptedTransport { get; set; } = false;

        /// <summary> [Optional] Path to certificate file. </summary>
        public const string CertificateFilePath = @"<path to certificate file>";

        /// <summary> [Optional] Path to certificate key file. </summary>
        public const string CertificateKeyFilePath = @"<path to certificate key file>";

        private Grpc.Core.Server server = null;
        private const string MicrosoftFolderInAppData = "Microsoft";

        /// <summary>
        /// Connector Name
        /// </summary>
        private const string ConnectorName = "CustomConnector"; // Replace this with the connector name

        /// <summary>
        /// API to initialize and start the logger
        /// </summary>
        public void StartLogger()
        {
            InitializeLogger();
            Log.Information("Starting Connector Logger");
        }

        /// <summary>
        /// API to initialize and start grpc server
        /// </summary>
        public void Start()
        {
            Log.Information("Starting server ...");

            try
            {
                ServerCredentials creds = SslServerCredentials.Insecure;

                if (UseSslEncryptedTransport)
                {
                    ServerCredentials sslCredentials = BuildSslCredentials();
                    if (sslCredentials == null)
                    {
                        Log.Fatal("Failed to build SSL credentials. Cannot start server.");
                        return;
                    }

                    creds = sslCredentials;
                }

                this.server = new Grpc.Core.Server
                {
                    Services =
                    {
                        ConnectorInfoService.BindService(new ConnectorInfoServiceImpl()),
                        ConnectionManagementService.BindService(new ConnectionManagementServiceImpl()),
                        ConnectorCrawlerService.BindService(new ConnectorCrawlerServiceImpl()),
                        ConnectorOAuthService.BindService(new ConnectorOAuthServiceImpl()),
                    },
                    Ports = { new ServerPort("localhost", Port, creds) },
                };

                this.server.Start();
                Log.Information("Server started. Listening for calls from Graph connectors platform.");
                Log.Information($"ConnectorId: {ConnectorInfoServiceImpl.ConnectorUniqueId} running on port: {Port}");
            }
            catch (Exception ex)
            {
                Log.Fatal($"Failed to start server with exception: {ex}");
            }
        }

        /// <summary>
        /// Stops a running server instance
        /// </summary>
        public void Stop()
        {
            Log.Information("Stopping server ...");
            this.server?.ShutdownAsync().Wait();
        }

        // Use this section to build certificate based credentials
        // Certificate based credentials will be used for SSL encrypted communication between Graph connectors platform and Connector
        // Certificate and key is used in the connector code and certificate needs to be provided to Graph connector platform. <ToDo: Provide link to document on configuring GCA>
        private static ServerCredentials BuildSslCredentials()
        {
            ServerCredentials credentials = null;

            if (!File.Exists(CertificateFilePath))
            {
                Log.Error($"Certificate file not found at: {CertificateFilePath}");
                return null;
            }

            if (!File.Exists(CertificateKeyFilePath))
            {
                Log.Error($"Certificate key file not found at: {CertificateKeyFilePath}");
                return null;
            }

            try
            {
                string certificateContent = File.ReadAllText(CertificateFilePath);
                string certKeyFileContent = File.ReadAllText(CertificateKeyFilePath);
                var keyCertPair = new KeyCertificatePair(certificateContent, certKeyFileContent);
                credentials = new SslServerCredentials(new[] { keyCertPair });
            }
            catch (Exception ex)
            {
                Log.Fatal($"Failed to create grpc server credentials from certificate at: {CertificateFilePath}\nException: {ex}");
            }

            return credentials;
        }

        /// <summary>
        /// Initializes serilog logger.
        /// Serilog is just an option. Feel free to use any of the logging frameworks.
        /// </summary>
        private static void InitializeLogger()
        {
            long logFileSizeLimitInBytes = 10 * 1000 * 1000; // 10 MB
            var logFilePath = Path.Combine(GetLocalAppDataFolder(), @"Logs\ConnectorLog.log"); // By default AppData folder of the current user account is used to store logs. Developer can give an absolute path which user account can access

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, fileSizeLimitBytes: logFileSizeLimitInBytes, rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        /// <summary>
        /// Returns path for local appdata folder for current assembly
        /// </summary>
        /// <returns>Folder path</returns>
        private static string GetLocalAppDataFolder()
        {
            string localAppDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataFolder = Path.Combine(
                localAppDataFolderPath,
                MicrosoftFolderInAppData,
                ConnectorName);

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            return appDataFolder;
        }
    }
}