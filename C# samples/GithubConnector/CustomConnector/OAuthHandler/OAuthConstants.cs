// ---------------------------------------------------------------------------
// <copyright file="OAuthClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.OAuthHandler
{
    /// <summary>
    /// OAuth Constants
    /// </summary>
    public class OAuthConstants
    {
        /// <summary>
        /// redirect url
        /// </summary>
        public const string RedirectUrl = "http://localhost:30323"; // [Input Required] Must match the registered redirect URI

        /// <summary>
        /// Github auth url
        /// </summary>
        public const string GetGithubAuthUrl = @"https://github.com/login/oauth/authorize?client_id={0}&redirect_uri={1}&state={2}";

        /// <summary>
        /// Github token endpoint
        /// </summary>
        public const string GithubTokenEndpoint = "https://github.com/login/oauth/access_token";

        /// <summary>
        /// Github refresh token GrantType
        /// </summary>
        public const string GithubRefreshTokenGrantType = "refresh_token";
    }
}
