using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CronJobsForHefesto
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                })
                .Build();

            host.Run();
        }
    }
}

