using CronJobsForHefesto.Services;

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
            var newPathCovers = await _storageAccountService.UploadToStorageAccountAsync(originalCovers);
            _notionService.UpdatePageCover(newPathCovers);
            /*
             * 1 - Garantir que está funcionando com o Docker
             * 2 - Proteger as secrets
             * 3 - Notificar execução ou falha
             * 4 - Instagram Service
             * 5 - Manifesto para Hefesto
             * 6 - Ter rodando no Hefesto
             */
        }
    }
}
