using CustomConnector.Server;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading;
using System.Threading.Tasks;

namespace CustomConnectorWorkerService
{
    public class Worker : BackgroundService
    {
        public Worker(ILogger<Worker> logger)
        {
            var server = new ConnectorServer();
            server.Start();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
    }
}
