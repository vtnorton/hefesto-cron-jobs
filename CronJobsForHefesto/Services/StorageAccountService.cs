using Azure.Storage.Blobs;
using CronJobsForHefesto.Models;

namespace CronJobsForHefesto.Services
{
    public class StorageAccountService
    {
        private readonly IHttpClientFactory _clientFactory;

        public StorageAccountService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<List<PageCover>> UploadToStorageAccountAsync(List<PageCover> originalCovers)
        {
            var pagesWithNewPath = new List<PageCover>();

            foreach (var cover in originalCovers)
            {
                var pageWithNewPath = await UploadPageCoverAsync(cover);
                pagesWithNewPath.Add(pageWithNewPath);
            }

            return pagesWithNewPath;
        }

        private async Task<PageCover> UploadPageCoverAsync(PageCover originalCover)
        {
            var connection = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var containerName = "covers";


            var blobServiceClient = new BlobServiceClient(connection);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var image = await GetImageFromPathAsync(originalCover.PhotoURL);
            var blobClient = containerClient.GetBlobClient(originalCover.PageId + GetExtentionFromFilePath(originalCover.PhotoURL));

            using (var stream = new MemoryStream(image))
            {
                blobClient.Upload(stream, true);
            }

            return new PageCover()
            {
                PageId = originalCover.PageId,
                IsAlreadyStoraged = true
            };
        }

        private string GetExtentionFromFilePath(string filePath)
        {
            return Path.GetExtension(filePath).ToLower();
        }

        private async Task<byte[]> GetImageFromPathAsync(string path)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(path);
            var content = await response.Content.ReadAsByteArrayAsync();

            return content; 
        }
    }
}
