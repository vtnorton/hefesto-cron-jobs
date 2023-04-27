using CronJobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CronJobs
{
    public class CronJobs
    {
        private readonly ILogger _logger;

        public CronJobs(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CronJobs>();
        }

        [Function("ExtendsInstagramAcessKeyFor60Days")]
        public void Run([TimerTrigger("0 */5 * * * *")] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
