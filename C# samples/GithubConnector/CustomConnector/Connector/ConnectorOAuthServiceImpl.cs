// ---------------------------------------------------------------------------
// <copyright file="ConnectorOAuthServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using CustomConnector.OAuthHandler;
using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Serilog;
using System;
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
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request message containing data needed to refresh the token</param>
        /// <param name="context">GRPC caller context</param>
        /// <returns>Response containing the refreshed access token and other token related details</returns>
        public async override Task<RefreshAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest request, ServerCallContext context)
        {
            OAuthTokenResponse oAuthTokenResponse;
            try
            {
                var oAuthClient = new OAuthClient();
                var oAuthOptions = new OAuthOptions()
                {
                    ClientId = request.AuthenticationData.OAuth2ClientCredential.AppId,
                    ClientSecret = request.AuthenticationData.OAuth2ClientCredential.AppSecret,
                    RefreshToken = request.AuthenticationData.OAuth2ClientCredential.OAuth2ClientCredentialResponse.RefreshToken,
                };

                oAuthTokenResponse = await oAuthClient.ExchangeTokenAsync(oAuthOptions, isRefresh: true);
                Log.Information("Successfully generated refresh token");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while getting refresh token");
                return await Task.FromResult(new RefreshAccessTokenResponse
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.AuthenticationIssue,
                        StatusMessage = ex.Message
                    }
                });
            }

            return await Task.FromResult(new RefreshAccessTokenResponse
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.Success
                },
                RefreshedCredentialData = new OAuth2ClientCredentialResponse
                {
                    AccessToken = oAuthTokenResponse.AccessToken,
                    ExpiresIn = 300,//(ulong)oAuthTokenResponse.ExpiresIn,
                    RefreshToken = oAuthTokenResponse.RefreshToken,
                    Scope = oAuthTokenResponse.Scope,
                    TokenType = oAuthTokenResponse.TokenType,
                }
            });
        }
    }
}
