// ---------------------------------------------------------------------------
// <copyright file="ConnectorOAuthServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using Grpc.Core;

using Microsoft.Graph.Connectors.Contracts.Grpc;

using System.Threading.Tasks;

using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectorOAuthService;

namespace CustomConnector.Connector
{
    /// <summary>
    /// Extends and implements connector OAuth APIs
    /// </summary>
    public class ConnectorOAuthServiceImpl : ConnectorOAuthServiceBase
    {
        /// <summary>
        /// Method to refresh the token using the refresh token or id token or using the credentials of app id and app secret
        /// </summary>
        /// <param name="request">Request message containing data needed to refresh the token</param>
        /// <param name="context">GRPC caller context</param>
        /// <returns>Response containing the refreshed access token and other token related details</returns>
        public override Task<RefreshAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest request, ServerCallContext context)
        {
            return Task.FromResult(new RefreshAccessTokenResponse
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.TokenExpired,
                    StatusMessage = "RefreshAccessToken is unimplemented",
                },
            });
        }
    }
}
