// ---------------------------------------------------------------------------
// <copyright file="ConnectorCrawlerServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using CustomConnector.Data;
using CustomConnector.Models;
using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectorCrawlerService;

namespace CustomConnector.Connector
{
    /// <summary>
    /// Class to implement crawl APIs needed to read data from datasource and pass it onto Graph connector platform
    /// </summary>
    public class ConnectorCrawlerServiceImpl : ConnectorCrawlerServiceBase
    {
        /// <summary>
        /// API to crawl datasource
        /// Expectation is to crawl datasource from the checkpoint provided and send the crawlItem
        /// Keep updating checkpoint info with every crawlItem so that Graph connector platform can try to resume crawl in-case of a crash or failure
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetCrawlStream(GetCrawlStreamRequest request, IServerStreamWriter<CrawlStreamBit> responseStream, ServerCallContext context)
        {
            try
            {

                Log.Information("GetCrawlStream Entry");
                int paginationCheckpoint = 1;
                if (Int32.TryParse(request.CrawlProgressMarker.CustomMarkerData, out int result))
                {
                    paginationCheckpoint = result;
                }

                //-------------------------------------------------------------------------------------------------------------//
                // This is a sample to read additional parameters from the custom configuration.
                // For example:
                //{
                //    "QueryParameters" : {
                //        "state" : "open"
                //    }
                //}
                // This is the json that is passed in the custom configuration field while creating the connection.
                var additionalQueryParams = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(request.CustomConfiguration.Configuration))
                {
                    var additionalParams = JsonSerializer.Deserialize<ProviderParams>(request.CustomConfiguration.Configuration);
                    additionalQueryParams = additionalParams.AdditionalParameters.QueryParameters;
                }
                // This is just a sample and can be modified as per the needs of the connector.
                //------------------------------------------------------------------------------------------------------------//

                var crawlItems = new List<CrawlItem>();
                bool itemsRemaining = true;
                while (itemsRemaining)
                {
                    var dataLoader = new DataLoader();
                    (crawlItems, itemsRemaining) = await dataLoader.GetCrawlItems(request.AuthenticationData, additionalQueryParams, paginationCheckpoint);
                    IEnumerator<CrawlItem> crawlitemEnumerator = crawlItems.GetEnumerator();
                    while (crawlitemEnumerator.MoveNext())
                    {
                        CrawlStreamBit crawlStreamBit = this.GetCrawlStreamBit(crawlitemEnumerator.Current, paginationCheckpoint);
                        await responseStream.WriteAsync(crawlStreamBit).ConfigureAwait(false);
                    }

                    paginationCheckpoint++;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Log.Error(ex.ToString());
                CrawlStreamBit crawlStreamBit = new CrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.TokenExpired, // this is needed to refresh the token
                        StatusMessage = "Authentication failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.NoRetry,
                        },
                    },
                };
                await responseStream.WriteAsync(crawlStreamBit).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                CrawlStreamBit crawlStreamBit = new CrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.DatasourceError,
                        StatusMessage = "Fetching items from datasource failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.Standard,
                        },
                    },
                };
                await responseStream.WriteAsync(crawlStreamBit).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// API to crawl datasource from the point where last incremental crawl ended
        /// Expectation is to crawl datasource from the checkpoint provided and send the items which are added/modified or deleted since the last incremental crawl.
        /// Keep updating checkpoint info with every crawlItem so that Graph connector platform will send this checkpoint for the next incremental crawl.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetIncrementalCrawlStream(GetIncrementalCrawlStreamRequest request, IServerStreamWriter<IncrementalCrawlStreamBit> responseStream, ServerCallContext context)
        {
            try
            {
                Log.Information("GetIncrementalCrawlStream Entry");
                int paginationCheckpoint = 1;
                DateTime lastModifiedAt = request.PreviousCrawlStartTimeInUtc.ToDateTime();
                if (DateTime.TryParse(request.CrawlProgressMarker.CustomMarkerData, out DateTime result))
                {
                    lastModifiedAt = result;
                }

                //-------------------------------------------------------------------------------------------------------------//
                // This is a sample to read additional parameters from the custom configuration.
                // For example:
                //{
                //    "QueryParameters" : {
                //        "state" : "open"
                //    }
                //}
                // This is the json that is passed in the custom configuration field while creating the connection.
                var additionalQueryParams = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(request.CustomConfiguration.Configuration))
                {
                    var additionalParams = JsonSerializer.Deserialize<ProviderParams>(request.CustomConfiguration.Configuration);
                    additionalQueryParams = additionalParams.AdditionalParameters.QueryParameters;
                }
                // This is just a sample and can be modified as per the needs of the connector.
                //------------------------------------------------------------------------------------------------------------//

                var incCrawlItems = new List<IncrementalCrawlItem>();
                bool itemsRemaining = true;
                while (itemsRemaining)
                {
                    var dataLoader = new DataLoader();
                    (incCrawlItems, itemsRemaining, lastModifiedAt) = await dataLoader.GetIncrementalCrawlItems(request.AuthenticationData, additionalQueryParams, paginationCheckpoint, lastModifiedAt);
                    IEnumerator<IncrementalCrawlItem> incCrawlitemEnumerator = incCrawlItems.GetEnumerator();
                    while (incCrawlitemEnumerator.MoveNext())
                    {
                        IncrementalCrawlStreamBit incCrawlStreamBit = this.GetIncrementalCrawlStreamBit(incCrawlitemEnumerator.Current, lastModifiedAt);
                        await responseStream.WriteAsync(incCrawlStreamBit).ConfigureAwait(false);
                    }

                    paginationCheckpoint++;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Log.Error(ex.ToString());
                IncrementalCrawlStreamBit incCrawlStreamBit = new IncrementalCrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.TokenExpired, // this is needed to refresh the token
                        StatusMessage = "Authentication failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.NoRetry,
                        },
                    },
                };
                await responseStream.WriteAsync(incCrawlStreamBit).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                IncrementalCrawlStreamBit incCrawlStreamBit = new IncrementalCrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.DatasourceError,
                        StatusMessage = "Fetching items from datasource failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.Standard,
                        },
                    },
                };
                await responseStream.WriteAsync(incCrawlStreamBit).ConfigureAwait(false);
            }

        }

        private CrawlStreamBit GetCrawlStreamBit(CrawlItem crawlItem, int paginationCheckpoint)
        {
            if (crawlItem != null)
            {
                return new CrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.Success,
                    },
                    CrawlItem = crawlItem,
                    CrawlProgressMarker = new CrawlCheckpoint
                    {
                        CustomMarkerData = paginationCheckpoint.ToString(),
                    },
                };
            }

            return new CrawlStreamBit
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.DatasourceError,
                    StatusMessage = "Fetching items from datasource failed",
                    RetryInfo = new RetryDetails
                    {
                        Type = RetryDetails.Types.RetryType.Standard,
                    },
                },
            };
        }

        private IncrementalCrawlStreamBit GetIncrementalCrawlStreamBit(IncrementalCrawlItem incCrawlItem, DateTime lastModifiedAt)
        {
            if (incCrawlItem != null)
            {
                return new IncrementalCrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.Success,
                    },
                    CrawlItem = incCrawlItem,
                    CrawlProgressMarker = new CrawlCheckpoint
                    {
                        CustomMarkerData = lastModifiedAt.ToUniversalTime().ToString(),
                    },
                };
            }

            return new IncrementalCrawlStreamBit
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.DatasourceError,
                    StatusMessage = "Fetching items from datasource failed",
                    RetryInfo = new RetryDetails
                    {
                        Type = RetryDetails.Types.RetryType.Standard,
                    },
                },
            };
        }
    }
}
