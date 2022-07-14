using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.Graph.Connectors.Contracts.Grpc;

using static Microsoft.Graph.Connectors.Contracts.Grpc.SourcePropertyDefinition.Types;


namespace CustomConnector.Models
{
    public class AppliancePart
    {
        [Key]
        public int PartNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Inventory { get; set; }
        public List<string> Appliances { get; set; }

        public static DataSourceSchema GetSchema()
        {
            DataSourceSchema schema = new DataSourceSchema();

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(PartNumber),
                    Type = SourcePropertyType.Int64,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Name),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Price),
                    Type = SourcePropertyType.Double,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Inventory),
                    Type = SourcePropertyType.Int64,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Appliances),
                    Type = SourcePropertyType.StringCollection,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Description),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            return schema;
        }

        public CrawlItem ToCrawlItem()
        {
            return new CrawlItem
            {
                ItemType = CrawlItem.Types.ItemType.ContentItem,
                ItemId = this.PartNumber.ToString(CultureInfo.InvariantCulture),
                ContentItem = this.GetContentItem(),
            };
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
                nameof(this.PartNumber),
                new GenericType
                {
                    IntValue = this.PartNumber,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Name),
                new GenericType
                {
                    StringValue = this.Name,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Price),
                new GenericType
                {
                    DoubleValue = this.Price,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Inventory),
                new GenericType
                {
                    IntValue = this.Inventory,
                });

            var appliancesPropertyValue = new StringCollectionType();
            foreach (var property in this.Appliances)
            {
                appliancesPropertyValue.Values.Add(property);
            }
            sourcePropertyValueMap.Values.Add(
                nameof(this.Appliances),
                new GenericType
                {
                    StringCollectionValue = appliancesPropertyValue,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Description),
                new GenericType
                {
                    StringValue = Description,
                });
                
            return sourcePropertyValueMap;
        }

    }
}
