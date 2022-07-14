// ---------------------------------------------------------------------------
// <copyright file="ConnectorCrawlerServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using Grpc.Core;

using Microsoft.Graph.Connectors.Contracts.Grpc;

using Serilog;

using System;
using System.Threading.Tasks;
using CustomConnector.Data;

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
                var crawlItems = CsvDataLoader.GetCrawlItemsFromCsv(request.AuthenticationData.DatasourceUrl);
                foreach (var crawlItem in crawlItems)
                {
                    CrawlStreamBit crawlStreamBit = this.GetCrawlStreamBit(crawlItem);
                    await responseStream.WriteAsync(crawlStreamBit).ConfigureAwait(false);
                }
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
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetIncrementalCrawlStream(GetIncrementalCrawlStreamRequest request, IServerStreamWriter<IncrementalCrawlStreamBit> responseStream, ServerCallContext context)
        {
            Log.Information("GetIncrementalCrawlStream Entry");

            // Placeholder code to remove compiler errors
            await Task.FromResult(true).ConfigureAwait(true);

            throw new RpcException(
                       new Status(
                           StatusCode.Unimplemented,
                           "'GetIncrementalCrawlStream' is not implemented."));
        }

        private CrawlStreamBit GetCrawlStreamBit(CrawlItem crawlItem)
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
                    CustomMarkerData = crawlItem.ItemId,
                },
            };
        }

    }
}
