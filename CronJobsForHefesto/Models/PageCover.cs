using CronJobsForHefesto.Enums;

namespace CronJobsForHefesto.Models
{
    public class PageCover
    {
        public string PageId { get; set; }
        public string PageLink  { get; set; }
        public string PhotoURL { get; set; }
        public NotionDatabase Database { get; set; }
        public bool IsAlreadyStoraged { get { return PhotoURL.StartsWith("https://vtnphotoswebsite.blob.core.windows.net/covers/"); } }
    }
}
