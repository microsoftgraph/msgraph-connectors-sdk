#### About
This is a sample project that provides an example for creating a custom connector that can run on Microsoft Graph connectors platform (GCP). The project builds on the contracts provided by Microsoft to interact with its GCP. More can be read on the Custom Graph connectors and its usage here: [Setup overview](https://docs.microsoft.com/en-us/microsoftsearch/configure-connector "Setup overview")

#### The project includes:

   1. A Connector server that listens to queries from GCP
   2. Contracts defined by platform for interoperability
   3. Stub implementation for all the APIs defined in the contract
   4. Sample code to demonstrate Model conversions between datasource models and contract defined models

Project is built as a .net core based console application. Once configured correctly GCP will be able make calls into this application through GRPC protocol. The primary responsibility of the connector code is to help GCP during connection creation and later during crawling to fetch information from the datasource. **Ensure the process is always running.** Refer here [On-Premises Agent | Microsoft Docs](https://docs.microsoft.com/en-us/MicrosoftSearch/graph-connector-agent "On-Premises Agent | Microsoft Docs") for configuring GCP to work with connector.

#### Execution flow:

1. First step is connection creation flow. This happens on Microsoft Admin portal where Search admin goes through a series of steps to configure a connection. Many of these steps end up making calls into the connector code in the following order.
	- ConnectionManagementServiceImpl.ValidateAuthentication
	- ConnectionManagementServiceImpl.ValidateCustomConfiguration
	- ConnectionManagementServiceImpl.GetDataSourceSchema

2. Once the connection is created successfully, GCP will start calling crawler API to crawl the datasource and return the items. ConnectorCrawlerServiceImpl.GetCrawlStream would be called to read data from data source. A sample implementation for a simple database access is provided for reference.

#### Tutorial: 
<Add link>

#### How to test code:
1. Install GCP and follow the instructions to register the agent [On-Premises Agent | Microsoft Docs](https://docs.microsoft.com/en-us/MicrosoftSearch/graph-connector-agent "On-Premises Agent | Microsoft Docs").
2. Build and run the console application.
3. Edit the CustomConnectorPortMap JSON file in the GCP installation folder (Program files > Graph connector agent) with connector id (same as connector id present in ConnectorInfoServiceImpl.cs) and TCP port information (Port used by connector can be found in: ConnectorServer.cs). This will be read by GCP while instantiating the connector instance. *You may need to open notepad/VS in admin mode to edit the JSON.*
4. Fill relevant details in the TestApp config files (AgentConfig.json and ConnectionInfo.json) inside the config folder(Program files> Graph connector agent > TestApp> Config)
	- You may need to open notepad/VS in admin mode to edit the JSON files.
	- Sample ConnectionInfo.json
		- Replace the providerId with connector id (Value is present in ConnectorInfoServiceImpl.cs).
		- Change the connection ID for each test app run.
5. Test out connector code flows using TestApp present in the GCA installation folder (Program files> Graph connector agent > TestApp> GraphConnectorAgentTest.exe)
	- TestApp is only for local testing.
	- Only one test case can be run at a time. You will have to exit and relaunch the app for testing another scenario.
