// ---------------------------------------------------------------------------
// <copyright file="ConnectorCrawlerServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using Serilog;
    using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectorCrawlerService;

    /// <summary>
    /// Class to implement crawl APIs needed to read data from datasource and pass it onto Graph connector platform
    /// </summary>
    public class ConnectorCrawlerServiceImpl : ConnectorCrawlerServiceBase
    {
        /// <summary>
        /// API to crawl datasource
        /// Expectation is to crawl datasource from the checkpoint provided and send the crawlItem
        /// Keep updating checkpoint info with every crawlItem so that Graph connector platform can try to resume crawl incase of a crash or failure
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetCrawlStream(GetCrawlStreamRequest request, IServerStreamWriter<CrawlStreamBit> responseStream, ServerCallContext context)
        {
            // This sample crawler simulates reading a database to fetch employee records from employee table
            // It also demonstrate adding access control info to items
            if (responseStream == null)
            {
                Log.Fatal("Response stream is null in GetCrawlStream");
                throw new ArgumentNullException(nameof(responseStream));
            }

            if (string.IsNullOrWhiteSpace(request?.AuthenticationData?.DatasourceUrl) || request.AuthenticationData.BasicCredential == null)
            {
                Log.Error("Authentication data is incomplete in GetCrawlItemStream. Ending the crawl with error");
                CrawlStreamBit errorBit = this.BuildCrawlStreamBitForError(OperationResult.AuthenticationIssue, "Null or incomplete authentication data received from Graph platform", false);
                await responseStream.WriteAsync(errorBit).ConfigureAwait(false);
                return;
            }

            DatabaseReader dbReader = new DatabaseReader();
            dbReader.InitializeConnection(
                request.AuthenticationData.DatasourceUrl,
                request.AuthenticationData.BasicCredential.Username,
                request.AuthenticationData.BasicCredential.Secret);

            int lastEmployeeCrawled = 0;
            if (int.TryParse(request.CrawlProgressMarker?.CustomMarkerData, out int lastEmpId))
            {
                lastEmployeeCrawled = lastEmpId;
            }

            string contentProperty = request.Schema?.PropertyList.FirstOrDefault(
                p => (p.DefaultSearchAnnotations & (uint)SourcePropertyDefinition.Types.SearchAnnotations.IsContent) == (uint)SourcePropertyDefinition.Types.SearchAnnotations.IsContent)?.Name;

            // Keep crawling until all items are returned
            while (true)
            {
                List<CrawlItem> crawlItems;
                try
                {
                    // [Code Here] to call your implementation of crawler + model convertor
                    crawlItems = dbReader.FetchEmployees(lastEmployeeCrawled, contentProperty);
                }
                catch (Exception ex)
                {
                    // [Code Here] ToString properly handle exceptions and return specific messages based on exception type
                    string errMsg = $"Fetching DB items failed with exception: {ex}";
                    Log.Fatal(errMsg);
                    CrawlStreamBit errorBit = this.BuildCrawlStreamBitForError(OperationResult.DatasourceError, errMsg, wouldRetyrFixError: false);
                    await responseStream.WriteAsync(errorBit).ConfigureAwait(false);
                    return;
                }

                if (crawlItems.Count == 0)
                {
                    // Done with all records. End the function to notify end of stream
                    break;
                }

                var operationStatus = new OperationStatus()
                {
                    Result = OperationResult.Success,
                };

                IEnumerator<CrawlItem> crawlitemEnumerator = crawlItems.GetEnumerator();
                while (crawlitemEnumerator.MoveNext())
                {
                    var crawlStreamBit = new CrawlStreamBit
                    {
                        CrawlItem = crawlitemEnumerator.Current,
                        Status = operationStatus,

                        // [Code Here] to set the right checkpoint info
                        CrawlProgressMarker = new CrawlCheckpoint
                        {
                            CustomMarkerData = crawlitemEnumerator.Current.ItemId,
                        },
                    };

                    await responseStream.WriteAsync(crawlStreamBit).ConfigureAwait(false);
                    if (int.TryParse(crawlitemEnumerator.Current.ItemId, out int empId))
                    {
                        lastEmployeeCrawled = empId;
                    }
                }
            }
        }

        private CrawlStreamBit BuildCrawlStreamBitForError(OperationResult errorType, string errorMessage, bool wouldRetyrFixError)
        {
            CrawlStreamBit crawlStreamBit = new CrawlStreamBit()
            {
                Status = new OperationStatus()
                {
                    Result = errorType,
                    StatusMessage = errorMessage,
                },
            };

            if (wouldRetyrFixError)
            {
                // Retry details are built just to show the usage. Actual values should be based on your understanding of datasource and the error.
                // [Code Here] to update it to match your need. You may want to configure different retry logic for different error types
                crawlStreamBit.Status.RetryInfo = new RetryDetails()
                {
                    Type = RetryDetails.Types.RetryType.Standard,
                    NumberOfRetries = 3,
                };
            }

            return crawlStreamBit;
        }
    }
}
