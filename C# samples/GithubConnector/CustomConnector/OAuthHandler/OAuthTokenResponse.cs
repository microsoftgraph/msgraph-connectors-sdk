// ---------------------------------------------------------------------------
// <copyright file="OAuthTokenResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.OAuthHandler
{
    using IdentityModel.Client;

    /// <summary>
    /// OAuth token response
    /// </summary>
    public class OAuthTokenResponse
    {
        /// <summary>
        /// Initialize an instance of <see cref="OAuthTokenResponse"/>
        /// </summary>
        /// <param name="tokenResponse">token response</param>
        public OAuthTokenResponse(TokenResponse tokenResponse)
        {
            this.AccessToken = tokenResponse.AccessToken;
            this.IdentityToken = tokenResponse.IdentityToken;
            this.TokenType = tokenResponse.TokenType;
            this.RefreshToken = tokenResponse.RefreshToken;
            this.ErrorDescription = tokenResponse.ErrorDescription;
            this.ExpiresIn = tokenResponse.ExpiresIn;
            this.Scope = tokenResponse.Scope;
        }

        /// <inheritdoc />
        public string AccessToken { get; set; }

        /// <inheritdoc />
        public string IdentityToken { get; set; }

        /// <inheritdoc />
        public string TokenType { get; set; }

        /// <inheritdoc />
        public string RefreshToken { get; set; }

        /// <inheritdoc />
        public string ErrorDescription { get; set; }

        /// <inheritdoc />
        public int? ExpiresIn { get; set; }

        /// <inheritdoc />
        public string Scope { get; set; }
    }
}
