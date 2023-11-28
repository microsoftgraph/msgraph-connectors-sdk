#### About
This is a sample for crawling and indexing GitHub Issues.

#### The sample covers:
1. Crawling a REST datasource
2. Implementing OAuth mechanism for authentication
3. Implementing the refresh mechanism for OAuth.
4. Implementing Incremental crawl

#### Prerequisites:
1. Admin Access to a Github Organization with a Repository with issues.
2. A Github App configured within the organization and:
	- has access to the repository
	- has Read-only permission for Issues and Pull Requests
	- has the same call back url as configured in the code
	- user-to-server token expiration enabled
	
#### Execution flow:
1. First step is connection creation flow. This happens on Microsoft Admin portal where Search admin goes through a series of steps to configure a connection. Many of these steps end up making calls into the connector code in the following order.
	- ConnectionManagementServiceImpl.ValidateAuthentication
	- ConnectionManagementServiceImpl.ValidateCustomConfiguration
	- ConnectionManagementServiceImpl.GetDataSourceSchema

2. Once the connection is created successfully, GCA will start calling crawler APIs to crawl the datasource and return the items. ConnectorCrawlerServiceImpl.GetCrawlStream would be called to get all the items and ConnectorCrawlerServiceImpl.GetIncrementalStream would be called to get the changes.

3. Since OAuth is selected as the authentication type, ConnectorOAuthServiceImpl.RefreshAccessToken would be called to refresh the Access token.

#### How to test code:
1. Install GCA and follow the instructions to register the agent [Graph Connector Agent | Microsoft Docs](https://learn.microsoft.com/en-us/MicrosoftSearch/graph-connector-agent "Graph Connector Agent | Microsoft Docs").
2. Build and run the console application.
3. Edit the CustomConnectorPortMap JSON file in the GCA installation folder (Program files > Graph connector agent) with connector id (same as connector id present in ConnectorInfoServiceImpl.cs) and TCP port information (Port used by connector can be found in: ConnectorServer.cs). This will be read by GCA while instantiating the connector instance. *You may need to open notepad/VS in admin mode to edit the JSON.*
4. Fill relevant details in the TestApp config files (ConnectionInfo.json and Manifest.json) inside the config folder(Program files> Graph connector agent > TestApp> Config). Sample files are provided. More on Test Utility: [Test Utility](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-testapp "Test Utility") 
	- You may need to open notepad/VS in admin mode to edit the JSON files.
	- Sample ConnectionInfo.json
		- Replace the providerId with connector id (Value is present in ConnectorInfoServiceImpl.cs).
		- Modify the path with organization name and repository name.
		- Add the client id and client secret of the Github App.
		- Change the connection ID for each test app run.
	- Sample Manifest.json
		- Fill the connector id (Value is present in ConnectorInfoServiceImpl.cs).
5. Test out connector code flows using TestApp present in the GCA installation folder (Program files> Graph connector agent > TestApp> GraphConnectorAgentTest.exe)
	- TestApp is only for local testing.
	- Only one test case can be run at a time. You will have to exit and relaunch the app for testing another scenario.