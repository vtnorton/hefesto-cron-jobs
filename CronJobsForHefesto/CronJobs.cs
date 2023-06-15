using System;
using CronJobsForHefesto.Functions;
using CronJobsForHefesto.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CronJobsForHefesto
{
    public class CronJobs
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;

        public CronJobs(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
        {
            _logger = loggerFactory.CreateLogger<CronJobs>();
            _clientFactory = clientFactory;
        }

        [Function("ExtendsInstagramAcessKeyFor60Days")]
        public async Task ExtendsInstagramAcessKeyFor60DaysAsync([TimerTrigger("0 0 2 1,15 * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"▶️ UploadPhotoFromNotionToStorageAccount function execution started at: {DateTime.Now}");

            var service = new AccessTokenFunctions(_clientFactory);
            await service.RefreshInstagramToken();

            _logger.LogInformation($"✅ UploadPhotoFromNotionToStorageAccount function executed at: {DateTime.Now}");
        }

        [Function("UploadPhotoFromNotionToStorageAccount")]
        public async Task UploadPhotoFromNotionToStorageAccountAsync([TimerTrigger("0 15 2 1-31 * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"▶️ UploadPhotoFromNotionToStorageAccount function execution started at: {DateTime.Now}");

            var service = new NotionToStorageAccount(_clientFactory);
            await service.ExcuteActionAsync();

            _logger.LogInformation($"✅ UploadPhotoFromNotionToStorageAccount function executed at: {DateTime.Now}");
        }
    }
}
