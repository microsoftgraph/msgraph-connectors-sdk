// ---------------------------------------------------------------------------
// <copyright file="DataLoader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CustomConnector.Models;
    using Newtonsoft.Json;
    using Serilog;
    using Microsoft.Extensions.DependencyInjection;

    public class DataLoader
    {
        /// <summary>The HTTP client factory</summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoader"/>
        /// </summary>
        public DataLoader()
        {
            ServiceProvider serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            this.httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        public async Task<(List<CrawlItem>, bool)> GetCrawlItems(AuthenticationData authenticationData, int paginationCheckpoint)
        {
            try
            {
                var crawlItems = new List<CrawlItem>();
                bool itemsRemaining = false;
                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    int maxRetries = 3; // You can adjust the number of retries as needed
                    int currentRetry = 0;

                    while (currentRetry < maxRetries)
                    {
                        try
                        {
                            var datasourceUrl = CreateFinalUrl(authenticationData.DatasourceUrl, paginationCheckpoint);
                            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(datasourceUrl));
                            request = AddRequestHeaders(request, authenticationData);
                            var response = await httpClient.SendAsync(request);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                var issueList = JsonConvert.DeserializeObject<List<GithubIssues>>(responseString);

                                if (issueList.Count != 0)
                                {
                                    foreach (var issue in issueList)
                                    {
                                        crawlItems.Add(issue.ToCrawlItem());
                                    }

                                    itemsRemaining = true;
                                }
                                else
                                {
                                    Log.Information("No item left in datasource to crawl");
                                    itemsRemaining = false;
                                }

                                break; // Break out of the loop if the request is successful
                            }
                            else
                            {
                                // no retries in case of 403
                                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                                }

                                Log.Error(response.ReasonPhrase);

                                // Optionally, you can add a delay between retries using Task.Delay
                                await Task.Delay(1000); // 1 second delay before retrying
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Log.Error(ex, "An error occurred while processing the request");
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An error occurred while processing the request");
                        }

                        currentRetry++;
                    }

                    // Check if the maximum number of retries has been reached
                    if (currentRetry == maxRetries)
                    {
                        throw new HttpRequestException("Maximum number of retries reached.");
                    }
                }

                return (crawlItems, itemsRemaining);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public async Task<(List<IncrementalCrawlItem>, bool, DateTime)> GetIncrementalCrawlItems(AuthenticationData authenticationData, int paginationCheckpoint, DateTime lastModifiedAt)
        {
            try
            {
                var incCrawlItems = new List<IncrementalCrawlItem>();
                bool itemsRemaining = false;
                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    int maxRetries = 3; // You can adjust the number of retries as needed
                    int currentRetry = 0;

                    while (currentRetry < maxRetries)
                    {
                        try
                        {
                            var datasourceUrl = authenticationData.DatasourceUrl + "?since=" + lastModifiedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            datasourceUrl = CreateFinalUrl(datasourceUrl, paginationCheckpoint);
                            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(datasourceUrl));
                            request = AddRequestHeaders(request, authenticationData);
                            var response = await httpClient.SendAsync(request);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                var issueList = JsonConvert.DeserializeObject<List<GithubIssues>>(responseString);

                                if (issueList.Count != 0)
                                {
                                    foreach (var issue in issueList)
                                    {
                                        incCrawlItems.Add(issue.ToIncrementalCrawlItem());
                                        lastModifiedAt = issue.UpdatedAt;
                                    }

                                    itemsRemaining = true;
                                }
                                else
                                {
                                    Log.Information("No item left in datasource to crawl");
                                    itemsRemaining = false;
                                }

                                break; // Break out of the loop if the request is successful
                            }
                            else
                            {
                                // no retries in case of 403
                                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                                }

                                Log.Error(response.ReasonPhrase);

                                // Optionally, you can add a delay between retries using Task.Delay
                                await Task.Delay(1000); // 1 second delay before retrying
                            }
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Log.Error(ex, "An error occurred while processing the request");
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An error occurred while processing the request");
                        }

                        currentRetry++;
                    }

                    // Check if the maximum number of retries has been reached
                    if (currentRetry == maxRetries)
                    {
                        throw new HttpRequestException("Maximum number of retries reached.");
                    }
                }

                return (incCrawlItems, itemsRemaining, lastModifiedAt);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        private static string CreateFinalUrl(string datasourceUrl, int paginationCheckpoint)
        {
            string finalUrl = string.Empty;
            if (datasourceUrl.Contains('?'))
            {
                finalUrl = datasourceUrl + "&page=" + paginationCheckpoint.ToString() + "&per_page=100" + "&direction=asc" + "&sort=updated";
            }
            else
            {
                finalUrl = datasourceUrl + "?page=" + paginationCheckpoint.ToString() + "&per_page=100" + "&direction=asc" + "&sort=updated";
            }
            return finalUrl;
        }

        private static HttpRequestMessage AddRequestHeaders(HttpRequestMessage request, AuthenticationData authenticationData)
        {
            request.Headers.Add("Authorization", "Bearer " + authenticationData.OAuth2ClientCredential.OAuth2ClientCredentialResponse.AccessToken);
            request.Headers.Add("Accept", "application/vnd.github+json");
            request.Headers.Add("User-Agent", "CustomConnectorSampleGithubApp");
            return request;
        }
    }
}