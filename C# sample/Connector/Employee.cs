// ---------------------------------------------------------------------------
// <copyright file="Employee.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace CustomConnectorTemplate.Connector
{
    using System;

    /// <summary>
    /// Model object to represent an employee record in Database
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Property name of employee name
        /// </summary>
        public const string EmployeeNameProperty = "EmployeeName";

        /// <summary>
        /// Property name of employee manager id
        /// </summary>
        public const string EmployeeManagerIdProperty = "EmployeeManagerId";

        /// <summary>
        /// Property name of employee joining date
        /// </summary>
        public const string EmployeeJoiningDateProperty = "JoiningDate";

        /// <summary>Employee ID</summary>
        public long Id { get; set; }

        /// <summary>Name of the employee</summary>
        public string Name { get; set; }

        /// <summary>Employee joining date</summary>
        public DateTime JoiningDate { get; set; }

        /// <summary>ID of the manager of employee</summary>
        public long ManagerId { get; set; }

        /// <summary>ID of department to which the employee belongs</summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// Comma separated strings of AADIDs who have access to the record
        /// </summary>
        public string AccessList { get; set; }
    }
}
