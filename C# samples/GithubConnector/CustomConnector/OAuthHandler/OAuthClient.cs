// <copyright file="OAuthClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace CustomConnector.OAuthHandler
{
    using IdentityModel.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// OAuth Client
    /// </summary>
    public class OAuthClient
    {
        /// <summary>The HTTP client factory</summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/>
        /// </summary>
        public OAuthClient()
        {
            ServiceProvider serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            this.httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        /// <inheritdoc />
        public async Task<OAuthTokenResponse> ExchangeTokenAsync(OAuthOptions oAuthOptions, bool isRefresh = false)
        {
            TokenResponse res;
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                if (!isRefresh)
                {
                    Console.WriteLine("Exchanging token for code");
                    using (var tokenRequest = new AuthorizationCodeTokenRequest()
                    {
                        Address = OAuthConstants.GithubTokenEndpoint,
                        ClientId = oAuthOptions.ClientId,
                        ClientSecret = oAuthOptions.ClientSecret,
                        Code = oAuthOptions.Code,
                        RedirectUri = oAuthOptions.RedirectUri,
                        Parameters = (Parameters)oAuthOptions.ExtraTokenRequestParameters,
                    })
                    {
                        res = await httpClient.RequestAuthorizationCodeTokenAsync(tokenRequest).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Refresh flow
                    Console.WriteLine("Refreshing token");
                    using (var tokenRequest = new RefreshTokenRequest()
                    {
                        Address = OAuthConstants.GithubTokenEndpoint,
                        ClientId = oAuthOptions.ClientId,
                        ClientSecret = oAuthOptions.ClientSecret,
                        GrantType = OAuthConstants.GithubRefreshTokenGrantType,
                        RefreshToken = oAuthOptions.RefreshToken,
                        Parameters = (Parameters)oAuthOptions.ExtraTokenRequestParameters,
                    })
                    {
                        res = await httpClient.RequestRefreshTokenAsync(tokenRequest).ConfigureAwait(false);
                    }
                }
            }

            if (res.IsError)
            {
                string errorMsg = $"Error occurred when getting the access token. {res.ErrorDescription}";
                throw new HttpRequestException(errorMsg, null, statusCode: res.HttpResponse.StatusCode);
            }

            return new OAuthTokenResponse(res);
        }
    }
}
