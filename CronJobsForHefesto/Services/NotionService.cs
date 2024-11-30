using CronJobsForHefesto.Enums;
using CronJobsForHefesto.Models;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Notion.Client;

namespace CronJobsForHefesto.Services
{
    internal class NotionService
    {
        private readonly ILogger _logger;
        private NotionClient _notionClient;
        private readonly string _devRelDBId;
        private readonly string _talksDBId;

        public NotionService(ILogger logger)
        {
            _logger = logger;
            _devRelDBId = Environment.GetEnvironmentVariable("NOTION_DATABASE_ID");
            _talksDBId = Environment.GetEnvironmentVariable("NOTION_TALKS_DB_ID");
            _notionClient = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN")
            });
        }

        public async Task<List<PageCover>> GetPostCoversAsync(bool showStorageCovers = true)
        {
            var pagesCovers = new List<PageCover>();

            var selectPostFilter = new MultiSelectFilter("Type", contains: "Post");
            var selectChangelogFilter = new MultiSelectFilter("Type", contains: "Changelog");

            var selectQueryPoster = new StatusFilter("Status-post", equal: "Query");
            var selectFollowUpPoster = new StatusFilter("Status-post", equal: "Follow-up");
            var selectPublishedPoster = new StatusFilter("Status-post", equal: "Published");
            var statusFilter = new List<Filter> { selectQueryPoster, selectFollowUpPoster, selectPublishedPoster };

            var queryPostParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    or: statusFilter,
                    and: new List<Filter> { selectPostFilter }
                ),
                PageSize = 250,
                Sorts = new List<Sort> { new()
                {
                    Property = "Date",
                    Direction = Direction.Descending
                } }
            };

            var queryChangelogParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    and: new List<Filter> { selectChangelogFilter }
                ),
                PageSize = 250,
                Sorts = new List<Sort> { new()
                {
                    Property = "Date",
                    Direction = Direction.Descending
                } }
            };

            var posts = await GetCoversOfPagesAsync(NotionDatabase.DevRel,queryPostParams, showStorageCovers);
            var changelogs = await GetCoversOfPagesAsync(NotionDatabase.DevRel, queryChangelogParams, showStorageCovers);

            pagesCovers.AddRange(changelogs);
            pagesCovers.AddRange(posts);

            return pagesCovers;
        }

        public async Task<List<PageCover>> GetTalksCoversAsync(bool showStorageCovers = true)
        {
            var selectQueryPoster = new StatusFilter("Status", equal: "Apresentada");
            var selectFollowUpPoster = new StatusFilter("Status", equal: "Call 4 Pappers");
            var selectPublishedPoster = new StatusFilter("Status", equal: "Planos de fazer");
            var statusFilter = new List<Filter> { selectQueryPoster, selectFollowUpPoster, selectPublishedPoster };

            var queryTalksParams = new DatabasesQueryParameters
            {
                Filter = new CompoundFilter(
                    or: statusFilter
                ),
                PageSize = 250,
            };

            var talks = await GetCoversOfPagesAsync(NotionDatabase.Talks, queryTalksParams, showStorageCovers);

            return talks;
        }

        private async Task<List<PageCover>> GetCoversOfPagesAsync(NotionDatabase database, DatabasesQueryParameters queryParameters, bool showStorageCovers = true)
        {
            var pagesCovers = new List<PageCover>();
            string databaseId = string.Empty;

            if (database == NotionDatabase.Talks)
                databaseId = _talksDBId;

            if (database == NotionDatabase.DevRel)
                databaseId = _devRelDBId;

            var pages = await _notionClient.Databases.QueryAsync(databaseId, queryParameters);

            foreach (var item in pages.Results)
            {
                if (item.Cover != null)
                {
                    pagesCovers.Add(new PageCover()
                    {
                        PageId = item.Id,
                        PageLink = item.Url,
                        PhotoURL = GetCoverURL(item),
                        Database = database,
                    });
                    Console.WriteLine("Imported Notion page: " + item.Id);
                }

            }

            if (showStorageCovers)
                return pagesCovers.ToList();

            return pagesCovers.Where(x => !x.IsAlreadyStoraged).ToList();
        }

        private string GetCoverURL(dynamic item)
        {
            if (item.Cover is ExternalFile externalCover && externalCover.External.Url != null)
                return externalCover.External.Url;

            if (item.Cover is UploadedFile coverFile && coverFile.File.Url != null)
                return item.Cover.File.Url;

            return "";
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
            var external = new ExternalFile.Info()
            {
                Url = pageCover.PhotoURL
            };
            var cover = new ExternalFile()
            {
                External = external
            };
            var toUpdate = new PagesUpdateParameters()
            {
                Cover = cover
            };
            _notionClient.Pages.UpdateAsync(pageCover.PageId, toUpdate);
            _logger.LogInformation("▶️ Updated page: " + pageCover.PageId + " (" + pageCover.PageLink + ")");
        }
    }
}