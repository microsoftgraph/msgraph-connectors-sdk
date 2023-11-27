// ---------------------------------------------------------------------------
// <copyright file="GithubIssues.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnector.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using Newtonsoft.Json;
    using Serilog;
    using static Microsoft.Graph.Connectors.Contracts.Grpc.SourcePropertyDefinition.Types;

    public class GithubIssues
    {
        [Key]
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("url", Required = Required.Always)]
        public string Url { get; set; }

        [JsonProperty("node_id", Required = Required.Always)]
        public string NodeId { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("state", Required = Required.Always)]
        public string State { get; set; }

        [JsonProperty("locked", Required = Required.Always)]
        public bool Locked { get; set; }

        [JsonProperty("labels", DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Labels { get; set; }

        [JsonProperty("comments", Required = Required.Always)]
        public int Comments { get; set; }

        [JsonProperty("created_at", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [DefaultValue(null)]
        [JsonProperty("updated_at", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DateTime UpdatedAt { get; set; }

        [DefaultValue(null)]
        [JsonProperty("closed_at", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DateTime? ClosedAt { get; set; }

        [DefaultValue("")]
        [JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Always)]
        public string Body { get; set; }

        public static DataSourceSchema GetSchema()
        {
            DataSourceSchema schema = new DataSourceSchema();

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Id),
                    Type = SourcePropertyType.Int64,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Url),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(NodeId),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Title),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(State),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Locked),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Body),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Comments),
                    Type = SourcePropertyType.Int64,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Labels),
                    Type = SourcePropertyType.StringCollection,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(CreatedAt),
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(UpdatedAt),
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(ClosedAt),
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            return schema;
        }

        public CrawlItem ToCrawlItem()
        {
            try
            {
                return new CrawlItem
                {
                    ItemType = CrawlItem.Types.ItemType.ContentItem,
                    ItemId = this.Id.ToString(CultureInfo.InvariantCulture),
                    ContentItem = this.GetContentItem(),
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        public IncrementalCrawlItem ToIncrementalCrawlItem()
        {
            try
            {
                return new IncrementalCrawlItem
                {
                    ItemType = IncrementalCrawlItem.Types.ItemType.ContentItem,
                    ItemId = this.Id.ToString(CultureInfo.InvariantCulture),
                    ContentItem = this.GetContentItem(),
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }



        private ContentItem GetContentItem()
        {
            return new ContentItem
            {
                AccessList = this.GetAccessControlList(),
                PropertyValues = this.GetSourcePropertyValueMap()
            };
        }

        private AccessControlList GetAccessControlList()
        {
            AccessControlList accessControlList = new AccessControlList();
            accessControlList.Entries.Add(this.GetAllowEveryoneAccessControlEntry());
            return accessControlList;
        }

        private AccessControlEntry GetAllowEveryoneAccessControlEntry()
        {
            return new AccessControlEntry
            {
                AccessType = AccessControlEntry.Types.AclAccessType.Grant,
                Principal = new Principal
                {
                    Type = Principal.Types.PrincipalType.Everyone,
                    IdentitySource = Principal.Types.IdentitySource.AzureActiveDirectory,
                    IdentityType = Principal.Types.IdentityType.AadId,
                    Value = "EVERYONE",
                }
            };
        }

        private SourcePropertyValueMap GetSourcePropertyValueMap()
        {
            SourcePropertyValueMap sourcePropertyValueMap = new SourcePropertyValueMap();

            sourcePropertyValueMap.Values.Add(
                nameof(this.Id),
                new GenericType
                {
                    IntValue = this.Id,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Url),
                new GenericType
                {
                    StringValue = this.Url,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Title),
                new GenericType
                {
                    StringValue = this.Title,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.NodeId),
                new GenericType
                {
                    StringValue = this.NodeId,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.State),
                new GenericType
                {
                    StringValue = this.State,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Body),
                new GenericType
                {
                    StringValue = this.Body,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Locked),
                new GenericType
                {
                    BoolValue = this.Locked,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Comments),
                new GenericType
                {
                    IntValue = this.Comments,
                });

            var appliancesPropertyValue = new StringCollectionType();
            foreach (var property in this.Labels)
            {
                appliancesPropertyValue.Values.Add(property);
            }
            sourcePropertyValueMap.Values.Add(
                nameof(this.Labels),
                new GenericType
                {
                    StringCollectionValue = appliancesPropertyValue,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.CreatedAt),
                new GenericType
                {
                    DateTimeValue = Timestamp.FromDateTime(this.CreatedAt.ToUniversalTime()),
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.UpdatedAt),
                new GenericType
                {
                    DateTimeValue = Timestamp.FromDateTime(this.UpdatedAt.ToUniversalTime()),
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.ClosedAt),
                new GenericType
                {
                    DateTimeValue = this.ClosedAt != null ? Timestamp.FromDateTime(this.ClosedAt.Value.ToUniversalTime()) : null,
                });

            return sourcePropertyValueMap;
        }

    }
}
