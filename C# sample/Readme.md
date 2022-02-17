This is a sample project that provides an example for creating a custom connector that can run on Microsoft Graph connectors platform (GCP). The project builds on the contracts provided by Microsoft to interact with its GCP. More can be read on the Custom Graph connectors and its usage here: `<ToDo>`

The project includes

   1. A Connector server that listens to queries from GCP
   2. Contracts defined by platform for interoperability
   3. Stub implementation for all the APIs defined in the contract
   4. Sample code to demonstrate Model conversions between datasource models and contract defined models

Project is built as a .net core based console application. Once configured correctly GCP will be able make calls into this application through GRPC protocol. The primary responsibility of the connector code is to help GCP during connection creation and later during crawling to fetch information from the datasource. **Ensure the process is always running.** Refer here `<ToDo>` for configuring GCP to work with connector.

Execution flow:
    1. First step is connection creation flow. This happens on Microsoft Admin portal where Search admin goes through a series of steps to configure a connection. Many of these steps end up making calls into the connector code in the following order.
        a. ConnectionManagementServiceImpl.ValidateAuthentication
        b. ConnectionManagementServiceImpl.ValidateCustomConfiguration
        c. ConnectionManagementServiceImpl.GetDataSourceSchema
    2. Once the connection is created successfully, GCP will start calling crawler API to crawl the datasource and return the items. ConnectorCrawlerServiceImpl.GetCrawlStream would be called to read data from data source. A sample implementation for a simple database access is provided for reference.

Where to write code:
    1. Search for `[Code Here]` in the entire project. We have marked areas that need coding or inspection.
    2. Replace `DatabaseReader` and `Employee` with your datasource reader and models.
    3. Take care of all error cases that may arise while talking to datasource or validating inputs. Return proper error message and type in OperationStatus for all the APIs.

How to test code:
    `<ToDo>`
