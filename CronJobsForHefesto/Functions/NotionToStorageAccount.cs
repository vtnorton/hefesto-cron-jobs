using CronJobsForHefesto.Models;
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

        public void ExcuteAction()
        {
            //var originalCovers = _notionService.GetPageCovers(false);
            var originalCovers = new List<PageCover>() {
                new PageCover() {PageId= "ahfiheer", PhotoURL = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/42/Temp_plate.svg/1280px-Temp_plate.svg.png"},
                new PageCover() {PageId= "ahfieer", PhotoURL = "https://m.media-amazon.com/images/I/61fTTDmlr-L._AC_UF1000,1000_QL80_.jpg"},
                new PageCover() {PageId= "ahfeer", PhotoURL = "https://cdn.shopify.com/s/files/1/2636/4748/products/laser-temperature-non-contact-thermometer-gun-accessories-ryonet-524003.jpg"},
            };
            var newPathCovers = _storageAccountService.UploadToStorageAccountAsync(originalCovers);
            //_notionService.UpdatePageCover(newPathCovers);
        }
    }
}
