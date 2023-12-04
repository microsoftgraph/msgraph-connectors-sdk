// ---------------------------------------------------------------------------
// <copyright file="GithubIssues.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.Models
{
    using Newtonsoft.Json;

    public class AdditionalParams
    {
        [JsonProperty("Parameters", Required = Required.Always)]
        public Parameters Parameters { get; set; }
    }

    public class Parameters
    {
        [JsonProperty("ConnectionId", Required = Required.Always)]
        public string ConnectionId { get; set; }
    }
}
