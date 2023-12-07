// ---------------------------------------------------------------------------
// <copyright file="GithubIssues.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ProviderParams
    {
        [DefaultValue(null)]
        [JsonProperty("AdditionalParameters", DefaultValueHandling = DefaultValueHandling.Populate)]
        public AdditionalParameters AdditionalParameters { get; set; }
    }

    public class AdditionalParameters
    {
        [JsonProperty("QueryParameters", Required = Required.Always)]
        public Dictionary<string, object> QueryParameters { get; set; }
    }
}
