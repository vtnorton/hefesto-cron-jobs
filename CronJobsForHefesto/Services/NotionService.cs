using CronJobsForHefesto.Models;

namespace CronJobsForHefesto.Services
{
    internal class NotionService
    {
        public List<PageCover> GetPageCovers(bool showStorageCovers = true)
        {
            return null;
        }

        public void UpdatePageCover(List<PageCover> pageCovers)
        {
            foreach (var item in pageCovers)
            {
                UpdatePageCover(item);
            }
        }

        private void UpdatePageCover(PageCover pageCover)
        {

        }
    }
}
