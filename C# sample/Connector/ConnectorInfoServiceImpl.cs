﻿// ---------------------------------------------------------------------------
// <copyright file="ConnectorInfoServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate.Connector
{
    using System.Threading.Tasks;
    using Grpc.Core;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectorInfoService;

    /// <summary>
    /// Extends and implements connector info APIs
    /// </summary>
    public class ConnectorInfoServiceImpl : ConnectorInfoServiceBase
    {
        /// <summary>
        /// Primary identifies for the connector. Same ID to be used in Manifest for connector onboarding in Graph connectors platform and Microsoft Admin Center.
        /// Cannot be changed after connections are created. Changing it later would fail the connections created with older ID
        /// Make sure ID is valid GUID
        /// </summary>
        public const string ConnectorUniqueId = "guid1";

        /// <summary>
        /// Returns basic information about the connector.
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Instance of GetBasicConnectorInfoResponse with information</returns>
        public override Task<GetBasicConnectorInfoResponse> GetBasicConnectorInfo(GetBasicConnectorInfoRequest request, ServerCallContext context)
        {
            GetBasicConnectorInfoResponse response = new GetBasicConnectorInfoResponse()
            {
                ConnectorId = ConnectorUniqueId,
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Health check APIs regularly called from Graph connectors platform
        /// If Graph connectors platform notices a sequence of healthcheck failures, it assumes the server to be down and fails the connection.
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Health check response with health status</returns>
        public override Task<HealthCheckResponse> HealthCheck(HealthCheckRequest request, ServerCallContext context)
        {
            HealthCheckResponse response = new HealthCheckResponse();
            return Task.FromResult(response);
        }
    }
}
