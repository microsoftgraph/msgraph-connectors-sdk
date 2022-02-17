// ---------------------------------------------------------------------------
// <copyright file="DatabaseReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate.Connector
{
    using System;
    using System.Collections.Generic;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.Graph.Connectors.Contracts.Grpc;
    using Serilog;

    /// <summary>
    /// Class to demo reading from a datasource and converting datasource entity to Graph connector items
    /// </summary>
    public class DatabaseReader
    {
        /// <summary>
        /// This is just for the test sample with hard coded items.
        /// In actual datasource we shot crawl when all items are returned
        /// </summary>
        private const int MaxItemsToCrawl = 500;

        /// <summary>
        /// Initialize DB connection that can be utilized for multiple queries
        /// </summary>
        /// <param name="serverUrl">Database Server URL</param>
        /// <param name="userName">access username</param>
        /// <param name="secret">access secret for given username</param>
        /// <returns>True on successful connection creation</returns>
        public bool InitializeConnection(string serverUrl, string userName, string secret)
        {
            // We should create a connection to database and store the connection instance in an instance variable to firing queries in data fetch APIs
            // For this example we are not making actual DB queries. Hnce, not creating a DB connection instance
            if (serverUrl == null || userName == null || secret == null)
            {
                Log.Error("Argument null error in DatabaseReader.InitializeConnection");
            }

            return true;
        }

        /// <summary>
        /// Reads a list of employee records from DB and converts it into CrawlItems
        /// </summary>
        /// <param name="lastEmployeeId">Checkpoint of last employee id for offset</param>
        /// <param name="contentProperty">Property to be set as content</param>
        /// <returns>List of crawl items</returns>
        public List<CrawlItem> FetchEmployees(int lastEmployeeId, string contentProperty)
        {
            List<CrawlItem> crawlItemList = new List<CrawlItem>();

            List<Employee> employeeList = this.ReadFromDB(lastEmployeeId);
            foreach (var employee in employeeList)
            {
                crawlItemList.Add(this.ConvertEmpoyeeRecordToCrawlItem(employee, contentProperty));
            }

            return crawlItemList;
        }

        private CrawlItem ConvertEmpoyeeRecordToCrawlItem(Employee employee, string contentProperty)
        {
            CrawlItem crawlItem = new CrawlItem()
            {
                ItemId = $"{employee.Id}",
                ItemType = CrawlItem.Types.ItemType.ContentItem,
            };

            crawlItem.ContentItem = this.BuildContentItemFromEmployee(employee, contentProperty);
            return crawlItem;
        }

        private ContentItem BuildContentItemFromEmployee(Employee employee, string contentProperty)
        {
            ContentItem contentItem = new ContentItem();
            contentItem.PropertyValues = this.BuildPropertyValueMapFromEmployee(employee);
            contentItem.AccessList = this.BuildAccessListFromEmployee(employee);
            contentItem.Content = this.BuildContentFromEmployee(employee, contentProperty);
            return contentItem;
        }

        private SourcePropertyValueMap BuildPropertyValueMapFromEmployee(Employee employee)
        {
            // Name and types should match the values returned as part of 'ConnectionManagementServiceImpl.GetDatasourceSchema'
            SourcePropertyValueMap propertyValueMap = new SourcePropertyValueMap();
            propertyValueMap.Values.Add(Employee.EmployeeNameProperty, new GenericType()
            {
                StringValue = employee.Name,
            });
            propertyValueMap.Values.Add(Employee.EmployeeManagerIdProperty, new GenericType()
            {
                IntValue = employee.ManagerId,
            });
            propertyValueMap.Values.Add(Employee.EmployeeJoiningDateProperty, new GenericType()
            {
                DateTimeValue = Timestamp.FromDateTime(employee.JoiningDate.ToUniversalTime()),
            });

            return propertyValueMap;
        }

        private AccessControlList BuildAccessListFromEmployee(Employee employee)
        {
            AccessControlList accessList = new AccessControlList();

            string accessListStr = employee.AccessList;
            if (string.IsNullOrWhiteSpace(accessListStr))
            {
                // There are no entries in access list. This means no one has access to the record
                // In ideal scenario we should return this record with a 'DENY:EVERYONE' access
                Principal principal = new Principal()
                {
                    Type = Principal.Types.PrincipalType.Everyone,
                    IdentitySource = Principal.Types.IdentitySource.AzureActiveDirectory,
                    IdentityType = Principal.Types.IdentityType.AadId,
                    Value = "EVERYONE",
                };

                AccessControlEntry ace = new AccessControlEntry()
                {
                    AccessType = AccessControlEntry.Types.AclAccessType.Deny,
                    Principal = principal,
                };

                accessList.Entries.Add(ace);
                return accessList;
            }

            string[] accessGrantIdList = employee.AccessList.Split(",");
            foreach (var accessGrantId in accessGrantIdList)
            {
                Principal principal = new Principal()
                {
                    Type = Principal.Types.PrincipalType.User,
                    IdentitySource = Principal.Types.IdentitySource.AzureActiveDirectory,
                    IdentityType = Principal.Types.IdentityType.AadId,
                    Value = accessGrantId,
                };

                AccessControlEntry ace = new AccessControlEntry()
                {
                    AccessType = AccessControlEntry.Types.AclAccessType.Grant,
                    Principal = principal,
                };

                accessList.Entries.Add(ace);
            }

            return accessList;
        }

        private Content BuildContentFromEmployee(Employee employee, string contentProperty)
        {
            switch (contentProperty)
            {
                case Employee.EmployeeNameProperty:
                    return new Content
                    {
                        ContentType = Content.Types.ContentType.Text,
                        ContentValue = employee.Name,
                    };
                default:
                    return new Content();
            }
        }

        private List<Employee> ReadFromDB(int lastEmployeeId)
        {
            List<Employee> employeeList = new List<Employee>();

            // Psuedo-ExitCondition for sample code
            if (lastEmployeeId > MaxItemsToCrawl)
            {
                return employeeList;
            }

            // Assuming we are fetching 100 records from DB at once
            long endId = lastEmployeeId + 100;
            for (int empId = lastEmployeeId + 1; empId < endId; ++empId)
            {
                employeeList.Add(this.BuildEmployeeRecord(empId));
            }

            return employeeList;
        }

        /// <summary>
        /// Helper function to build dummy records
        /// </summary>
        /// <param name="empId">Employee ID</param>
        /// <returns>Employee record</returns>
        private Employee BuildEmployeeRecord(long empId)
        {
            // Build and return a hypothetical employee record
            return new Employee()
            {
                Id = empId,
                Name = $"Employee_{empId}",
                JoiningDate = (DateTime.Now - TimeSpan.FromDays(2000)).AddDays(empId),
                DepartmentId = empId % 3,
                ManagerId = empId + 10000,
            };
        }
    }
}
