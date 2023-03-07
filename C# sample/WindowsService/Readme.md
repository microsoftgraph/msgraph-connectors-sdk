#### About
This is a sample project that provides an example for running the custom connector as a Windows service.

#### To use this:

   1. Add this project in the same solution where your connector is present.
   2. Modify the project reference to your connector project. Modify the namespaces in this project if required to the namespaces of your connector.
   3. Build the project and run it as a windows service using this script:
            
            $ServiceName = "CustomConnector"
            $ExePath = "<Full path of CustomConnectorWorkerService.exe from above build>"
            # Create a service with the given executable. This just creates an entry for this service.
            sc.exe create $ServiceName binPath="$ExePath" start="delayed-auto"
            # Set the service to run under a virtual account NT Service\<ServiceName>. Optionally skip this step to run the service under LOCAL SYSTEM account
            sc.exe config $ServiceName obj="NT Service\$ServiceName"
            # Restarts service after 5 minutes on first, second and third failures and resets error after 1 day
            sc.exe failureflag $ServiceName 1
            sc.exe failure $ServiceName reset= 86400 actions= restart/300000/restart/300000/restart/300000
            sc.exe start $ServiceName

   4. Open services.msc and check that the service is in running state.
   5. You can refer documentation for troubleshooting issues with hosting the connector: [Troubleshoot Issues with the Microsoft Graph Connector SDK](https://learn.microsoft.com/en-us/graph/custom-connector-sdk-troubleshooting#troubleshooting-errors-while-hosting-the-connector-as-a-windows-service "Troubleshoot Issues with the Microsoft Graph Connector SDK")