// ---------------------------------------------------------------------------
// <copyright file="ConnectionManagementServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate.Connector
{
    using System.Threading.Tasks;

    using Grpc.Core;

    using Microsoft.Graph.Connectors.Contracts.Grpc;

    using Serilog;

    using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectionManagementService;
    using static Microsoft.Graph.Connectors.Contracts.Grpc.SourcePropertyDefinition.Types;

    /// <summary>
    /// Implements connections management APIs
    /// </summary>
    public class ConnectionManagementServiceImpl : ConnectionManagementServiceBase
    {
        /// <summary>
        /// Validates if the credentials provided by during connection creation are valid and allow us to access the speciied datasource.
        /// This is the first API called during connection creation process.
        /// </summary>
        /// <param name="request">Request containing all the authentication information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Response with validation result</returns>
        public override Task<ValidateAuthenticationResponse> ValidateAuthentication(ValidateAuthenticationRequest request, ServerCallContext context)
        {
            if (request?.AuthenticationData == null)
            {
                Log.Error("Got null authentication data in ValidateAuthentication. Failing Validation");
                return this.BuildAuthValidationResponse(false, "Authentication data is null");
            }

            if (string.IsNullOrWhiteSpace(request.AuthenticationData.DatasourceUrl))
            {
                Log.Error("Got null datasource URL data in ValidateAuthentication. Failing Validation");
                return this.BuildAuthValidationResponse(false, "Datasource URL data is null");
            }

            Log.Information($"Validating access using {request.AuthenticationData.AuthType}");

            // You can specify what auth types are supported by connector and write implementations only for them
            // This is a sample code to demonstrate validating if more than one auth types is supported by connector.
            switch (request.AuthenticationData.AuthType)
            {
                case AuthenticationData.Types.AuthenticationType.Anonymous:
                    return this.ValidateAnonymousAuth(request.AuthenticationData.DatasourceUrl);
                case AuthenticationData.Types.AuthenticationType.Basic:
                    return this.ValidateBasicAuthentication(request.AuthenticationData.DatasourceUrl, request.AuthenticationData.BasicCredential);
                case AuthenticationData.Types.AuthenticationType.Windows:
                    return this.ValidateWindowsAuthentication(request.AuthenticationData.DatasourceUrl, request.AuthenticationData.WindowsCredential);
            }

            return this.BuildAuthValidationResponse(false, "Unsupported authentication type");
        }

        private Task<ValidateAuthenticationResponse> ValidateAnonymousAuth(string datasourceUrl)
        {
            // Anonymous authentication standas for un authenticated access. For example public intranet websites that do not need any authentication to access
            // This is just a sample validation to show handling response.
            // [Code Here] to check if datasource URl is reachable without any authentication. Return a failure if it is not supported.
            bool accessSuccess = !string.IsNullOrWhiteSpace(datasourceUrl);

            return this.BuildAuthValidationResponse(accessSuccess);
        }

        private Task<ValidateAuthenticationResponse> ValidateBasicAuthentication(string datasourceUrl, BasicCredential basicCredential)
        {
            // Basic authentication uses simple username password based authentication
            // This sample shows how to use the basic authetication to validate.
            // [Code Here] to check if datasource is accessible with the provided credentials. Return a failure if it is not supported.
            bool accessSuccess = !(string.IsNullOrWhiteSpace(datasourceUrl)
                                    || basicCredential == null
                                    || string.IsNullOrWhiteSpace(basicCredential.Username)
                                    || string.IsNullOrWhiteSpace(basicCredential.Secret));

            return this.BuildAuthValidationResponse(accessSuccess);
        }

        private Task<ValidateAuthenticationResponse> ValidateWindowsAuthentication(string datasourceUrl, WindowsCredential windowsCredential)
        {
            // Windows authentication includes information on domain, username and secret.
            // These info can be used to impersonate threads to apply windows principal or pass along to datasource for further validation
            // [Code Here] to check if given datasource is accessible using credentials provided. Return a failure if it is not supported.
            bool accessSuccess = !(string.IsNullOrWhiteSpace(datasourceUrl)
                                    || windowsCredential == null
                                    || string.IsNullOrWhiteSpace(windowsCredential.Domain)
                                    || string.IsNullOrWhiteSpace(windowsCredential.Username)
                                    || string.IsNullOrWhiteSpace(windowsCredential.Secret));

            return this.BuildAuthValidationResponse(accessSuccess);
        }

        private Task<ValidateAuthenticationResponse> BuildAuthValidationResponse(bool accessSuccess, string errorMessageOnFailure = "")
        {
            Log.Information($"Building Auth validation response for {accessSuccess} with message: {errorMessageOnFailure}");
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
        /// The format and structure of the configuration is decded by the developer of connector and validation done here should be based on those definitions.
        /// This is an optional step in connection creation and can be ignored (return success) if there is no specific configuration needed to access datasource.
        /// </summary>
        /// <param name="request">Request with all required information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Validation status</returns>
        public override Task<ValidateCustomConfigurationResponse> ValidateCustomConfiguration(ValidateCustomConfigurationRequest request, ServerCallContext context)
        {
            Log.Information("Validating custom configuration");

            // [Code Here] to validate configuration. All the required information is part of the request object
            // Example on how to get connection info:
            //
            // string customConfig = request?.CustomConfiguration?.Configuration;
            // AuthenticationType? authType = request.AuthenticationData?.AuthType;
            // string datasourceUrl = request.AuthenticationData.DatasourceUrl;
            // BasicCredential credential = request.AuthenticationData.BasicCredential;
            //
            // For now we will be ruturning success assuming no custom config was expected and only authdata is needed
            //
            bool validationSuccess = request.AuthenticationData != null;

            Log.Information($"Custom configuration validation success = {validationSuccess}");
            OperationStatus validationStatus = null;
            if (validationSuccess)
            {
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.Success,
                };
            }
            else
            {
                // [Code Here] to capture right validation error details
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.ValidationFailure,
                    StatusMessage = "Validation failed",
                };
            }

            ValidateCustomConfigurationResponse response = new ValidateCustomConfigurationResponse()
            {
                Status = validationStatus,
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Returns schema of item retrieved from data source. Schema defines properties available to be read from datasource for individual entities.
        /// Ex: A file can have attributes like Filename, Extension, FileSize, ModifiedDate etc...
        /// Api is expected to return list of all properties available for datasource entities
        /// This is the third API called by Graph connectors service. Called after ValidateCustomConfiguration
        /// </summary>
        /// <param name="request">Request will all info to connect to datasource</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>List of properties available for datasource entities</returns>
        public override Task<GetDataSourceSchemaResponse> GetDataSourceSchema(GetDataSourceSchemaRequest request, ServerCallContext context)
        {
            Log.Information("Trying to fetch datasource schema");

            // [Code Here] to connect to datasource and read all the available properties of an entity in the datasource that can be ingested into Graph
            // You can also return a static list if the schema is same across all datasources irrespective of instance or other custom configuration.
            // Data returned as parting crawler APIs should return property values that conform to this schema. Anything outside of schema will be discarded.
            //
            // In this example we will be retuning a static list of 3 simple propeties for an employee.
            //
            // Datasource access and any other errors need to be handled and right error status to be returned.
            //
            SourcePropertyDefinition[] properties =
            {
                new SourcePropertyDefinition()
                {
                    Name = Employee.EmployeeNameProperty,
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsContent),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsContent),
                },
                new SourcePropertyDefinition()
                {
                    Name = Employee.EmployeeManagerIdProperty,
                    Type = SourcePropertyType.Int64,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable | SearchAnnotations.IsQueryable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable | SearchAnnotations.IsQueryable),
                },
                new SourcePropertyDefinition()
                {
                    Name = Employee.EmployeeJoiningDateProperty,
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable | SearchAnnotations.IsQueryable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable | SearchAnnotations.IsQueryable),
                },
            };

            DataSourceSchema schema = new DataSourceSchema();
            foreach (var property in properties)
            {
                schema.PropertyList.Add(property);
            }

            Log.Information($"Returning datasource schema with {schema.PropertyList.Count} properrties");

            OperationStatus opStatus = null;
            if (schema.PropertyList.Count > 0)
            {
                opStatus = new OperationStatus()
                {
                    Result = OperationResult.Success,
                };
            }
            else
            {
                // [Code Here] to capture the right error info incase of an error in reading schema from datasource
                opStatus = new OperationStatus()
                {
                    Result = OperationResult.DatasourceError,
                    StatusMessage = "Failed to fetch datasource schema",
                };
            }

            GetDataSourceSchemaResponse response = new GetDataSourceSchemaResponse()
            {
                DataSourceSchema = schema,
                Status = opStatus,
            };

            return Task.FromResult(response);
        }
    }
}
