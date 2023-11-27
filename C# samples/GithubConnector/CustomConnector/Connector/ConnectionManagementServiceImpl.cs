// ---------------------------------------------------------------------------
// <copyright file="ConnectionManagementServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using CustomConnector.Models;
using CustomConnector.OAuthHandler;
using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectionManagementService;

namespace CustomConnector.Connector
{
    /// <summary>
    /// Implements connection management APIs
    /// </summary>
    public class ConnectionManagementServiceImpl : ConnectionManagementServiceBase
    {
        /// <summary>
        /// Validates if the credentials provided during connection creation are valid and allow us to access the specified datasource.
        /// This is the first API called during connection creation process.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all the authentication information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Response with validation result</returns>
        public override async Task<ValidateAuthenticationResponse> ValidateAuthentication(ValidateAuthenticationRequest request, ServerCallContext context)
        {
            Log.Information("Validating Authentication");
            if (request.AuthenticationData.AuthType == AuthenticationData.Types.AuthenticationType.Oauth2ClientCredential)
            {
                return await this.BuildOAuthValidationResponse(request);
            }

            return await this.BuildAuthValidationResponse(false, errorMessageOnFailure: "Not Implemented");
        }

        private async Task<ValidateAuthenticationResponse> BuildOAuthValidationResponse(ValidateAuthenticationRequest request)
        {
            try
            {
                string clientId = request.AuthenticationData.OAuth2ClientCredential.AppId;
                string clientSecret = request.AuthenticationData.OAuth2ClientCredential.AppSecret;
                string redirectUri = OAuthConstants.RedirectUrl;
                string state = Guid.NewGuid().ToString();

                // Start a simple HTTP server to handle the GitHub OAuth callback
                var listener = new HttpListener();
                listener.Prefixes.Add(redirectUri + "/");
                listener.Start();

                Console.WriteLine($"Please wait for the authorization process to complete...");

                // Open the GitHub authorization URL in the default browser
                string githubAuthUrl = string.Format(CultureInfo.InvariantCulture, OAuthConstants.GetGithubAuthUrl, clientId, redirectUri, state);
                Process.Start(new ProcessStartInfo(githubAuthUrl)
                {
                    UseShellExecute = true,
                });

                // Wait for the callback and extract the authorization code
                var httpContext = await listener.GetContextAsync();
                var httpRequest = httpContext.Request;
                string authorizationCode = httpRequest.QueryString.Get("code");
                Console.WriteLine($"Authorization code: {authorizationCode}");

                // Stop the HTTP server
                listener.Stop();

                // Exchange the authorization code for an access token
                var oAuthClient = new OAuthClient();
                var oAuthOptions = new OAuthOptions()
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    RedirectUri = redirectUri,
                    Code = authorizationCode,
                };

                var oAuthTokenResponse = await oAuthClient.ExchangeTokenAsync(oAuthOptions);
                Log.Information("Successfully generated OAuth Token");
                return await this.BuildAuthValidationResponse(true, oAuthTokenResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while validating authentication");
                return await this.BuildAuthValidationResponse(false, errorMessageOnFailure: ex.Message);
            }
        }

        private Task<ValidateAuthenticationResponse> BuildAuthValidationResponse(bool accessSuccess, OAuthTokenResponse oAuthResponse = null, string errorMessageOnFailure = "")
        {
            Log.Information($"Building Authentication validation response for {accessSuccess} with message: {errorMessageOnFailure}");
            OperationStatus validationStatus = null;
            if (accessSuccess)
            {
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.Success,
                };
            }
            else
            {
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.AuthenticationIssue,
                    StatusMessage = errorMessageOnFailure,
                };
            }

            ValidateAuthenticationResponse response = new ValidateAuthenticationResponse()
            {
                Status = validationStatus,
            };

            if (oAuthResponse != null)
            {
                var oAuthClientCredentialResponse = new OAuth2ClientCredentialResponse()
                {
                    AccessToken = oAuthResponse.AccessToken,
                    RefreshToken = oAuthResponse.RefreshToken,
                    ExpiresIn = 300,//(ulong)oAuthResponse.ExpiresIn,
                    Scope = oAuthResponse.Scope,
                    TokenType = oAuthResponse.TokenType,
                };

                Console.WriteLine(oAuthResponse.AccessToken);
                response.OAuth2ClientCredentialResponse = oAuthClientCredentialResponse;
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Validates if the custom configuration provided during connection creation is valid and datasource can be accessed based on the configuration provided.
        /// This will be called after ValidateAuthentication from Graph connectors platform
        /// The format and structure of the configuration is decoded by the developer of connector and validation done here should be based on those definitions.
        /// This is an optional step in connection creation and can be ignored (return success) if there is no specific configuration needed to access datasource.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request with all required information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Validation status</returns>
        public override Task<ValidateCustomConfigurationResponse> ValidateCustomConfiguration(ValidateCustomConfigurationRequest request, ServerCallContext context)
        {
            Log.Information("Validating custom configuration");
            ValidateCustomConfigurationResponse response;

            if (!string.IsNullOrWhiteSpace(request.CustomConfiguration.Configuration))
            {
                response = new ValidateCustomConfigurationResponse()
                {
                    Status = new OperationStatus()
                    {
                        Result = OperationResult.ValidationFailure,
                        StatusMessage = "No additional parameters are required for this connector"
                    },
                };
            }
            else
            {
                response = new ValidateCustomConfigurationResponse()
                {
                    Status = new OperationStatus()
                    {
                        Result = OperationResult.Success,
                    },
                };
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Returns schema of item retrieved from data source. Schema defines properties available to be read from datasource for individual entities.
        /// Ex: A file can have attributes like Filename, Extension, FileSize, ModifiedDate etc...
        /// API is expected to return list of all properties available for datasource entities
        /// This is the third API called by Graph connectors service. Called after ValidateCustomConfiguration
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request will all info to connect to datasource</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>List of properties available for datasource entities</returns>
        public override Task<GetDataSourceSchemaResponse> GetDataSourceSchema(GetDataSourceSchemaRequest request, ServerCallContext context)
        {
            Log.Information("Trying to fetch datasource schema");

            var opStatus = new OperationStatus()
            {
                Result = OperationResult.Success,
            };

            GetDataSourceSchemaResponse response = new GetDataSourceSchemaResponse()
            {
                DataSourceSchema = GithubIssues.GetSchema(),
                Status = opStatus,
            };

            return Task.FromResult(response);
        }
    }
}
