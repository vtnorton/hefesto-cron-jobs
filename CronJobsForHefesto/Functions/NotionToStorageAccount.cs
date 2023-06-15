using CronJobsForHefesto.Services;
using System;

namespace CronJobsForHefesto.Functions
{
    public class NotionToStorageAccount
    {
        private NotionService _notionService;
        private StorageAccountService _storageAccountService;

        public NotionToStorageAccount(IHttpClientFactory clientFactory) {
            _notionService = new NotionService();
            _storageAccountService = new StorageAccountService(clientFactory);
        }

        public async Task ExcuteActionAsync()
        {
            var originalCovers = await _notionService.GetPageCoversAsync(false);
            if (originalCovers.Count == 0)
            {
                Console.WriteLine("✅ No image was found to be replaced");
                
            }else
            {
                var newPathCovers = await _storageAccountService.UploadToStorageAccountAsync(originalCovers);
                _notionService.UpdatePageCover(newPathCovers);
            }
        }
    }
}
