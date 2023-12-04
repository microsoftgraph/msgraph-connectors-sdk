// ---------------------------------------------------------------------------
// <copyright file="ConnectionManagementServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using Grpc.Core;

using Microsoft.Graph.Connectors.Contracts.Grpc;

using Serilog;

using System;
using System.Threading.Tasks;
using CustomConnector.Data;
using CustomConnector.Models;

using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectionManagementService;

namespace CustomConnector.Connector
{
    /// <summary>
    /// Implements connections management APIs
    /// </summary>
    public class ConnectionManagementServiceImpl : ConnectionManagementServiceBase
    {
        /// <summary>
        /// Validates if the credentials provided by during connection creation are valid and allow us to access the specified datasource.
        /// This is the first API called during connection creation process.
        /// </summary>
        /// <param name="request">Request containing all the authentication information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Response with validation result</returns>
        public override Task<ValidateAuthenticationResponse> ValidateAuthentication(ValidateAuthenticationRequest request, ServerCallContext context)
        {
            if (request.AuthenticationData.AuthType == AuthenticationData.Types.AuthenticationType.Basic)
            {
                Log.Information("Validating basic authentication");
                
                // make a datasource API call to validate the credentials. If the call fails, return false with appropriate error message
                // For example, if the datasource is a SQL database, try to connect to the database using the credentials provided
                // If the datasource is a REST API, try to make a call to the API using the credentials provided
                // Sample Code:
                // var response = await CsvDataLoader.ValidateBasicAuth(request.AuthenticationData.DatasourceUrl, request.AuthenticationData.BasicCredential.UserName, request.AuthenticationData.BasicCredential.Secret);
                // if (response.StatusCode == System.Net.HttpStatusCode.OK)
                // {
                //     return this.BuildAuthValidationResponse(true);
                // }
                // else
                // {
                //     return this.BuildAuthValidationResponse(false, "Could not validate the provided credentials");
                // }

                return this.BuildAuthValidationResponse(false, "Basic Authentication is not supported");
            }

            try
            {
                Log.Information("Validating anonymous authentication");
                CsvDataLoader.ReadRecordFromCsv(request.AuthenticationData.DatasourceUrl);
                return this.BuildAuthValidationResponse(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return this.BuildAuthValidationResponse(false, "Could not read the provided CSV file with the provided credentials");
            }
        }

        private Task<ValidateAuthenticationResponse> BuildAuthValidationResponse(bool accessSuccess, string errorMessageOnFailure = "")
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

            return Task.FromResult(response);
        }

        /// <summary>
        /// Validates if the custom configuration provided during connection creation is valid and datasource can be accessed based on the configuration provided.
        /// This will be called after ValidateAuthentication from Graph connectors platform
        /// The format and structure of the configuration is decoded by the developer of connector and validation done here should be based on those definitions.
        /// This is an optional step in connection creation and can be ignored (return success) if there is no specific configuration needed to access datasource.
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
                DataSourceSchema = AppliancePart.GetSchema(),
                Status = opStatus,
            };

            return Task.FromResult(response);
        }

    }
}
