// ---------------------------------------------------------------------------
// <copyright file="OAuthOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.OAuthHandler
{
    using System.Collections.Generic;

    /// <summary>
    /// OAuth options
    /// </summary>
    public class OAuthOptions
    {
        /// <summary>
        /// Client id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Redirect url
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Extra token parameters
        /// </summary>
        public IDictionary<string, string> ExtraTokenRequestParameters { get; set; }
    }
}
