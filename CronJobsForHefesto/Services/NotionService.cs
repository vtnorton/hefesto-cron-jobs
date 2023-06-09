﻿using CronJobsForHefesto.Models;
using Microsoft.Extensions.Logging;
using Notion.Client;

namespace CronJobsForHefesto.Services
{
    internal class NotionService
    {
        private readonly ILogger _logger;
        private NotionClient _notionClient;
        public NotionService(ILogger logger) {
            _logger = logger;
            _notionClient = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN")
            });
        }

        public async Task<List<PageCover>> GetPageCoversAsync(bool showStorageCovers = true)
        {
            var pagesCovers = new List<PageCover>();
            var databaseId = Environment.GetEnvironmentVariable("NOTION_DATABASE_ID");

            var selectPostFilter = new MultiSelectFilter("Type", contains: "Post");
            var selectChangelogFilter = new MultiSelectFilter("Type", contains: "Changelog");

            var selectQueryPoster = new SelectFilter("Status-post", equal: "Query");
            var selectFollowUpPoster = new SelectFilter("Status-post", equal: "Follow-up");
            var selectPublishedPoster = new SelectFilter("Status-post", equal: "Published");
            var statusFilter = new List<Filter> { selectQueryPoster, selectFollowUpPoster, selectPublishedPoster };

            var queryPostParams = new DatabasesQueryParameters { 
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

            var posts = await _notionClient.Databases.QueryAsync(databaseId, queryPostParams);
            var changelogs = await _notionClient.Databases.QueryAsync(databaseId, queryChangelogParams);

            foreach (var item in posts.Results)
            {
                if (item.Cover != null)
                {
                    pagesCovers.Add(new PageCover()
                    {
                        PageId = item.Id,
                        PageLink = item.Url,
                        PhotoURL = GetCoverURL(item)
                    });
                    Console.WriteLine("Imported Notion page: " + item.Id);
                }

            }

            foreach (var item in changelogs.Results)
            {
                if (item.Cover != null)
                {
                    pagesCovers.Add(new PageCover()
                    {
                        PageId = item.Id,
                        PageLink = item.Url,
                        PhotoURL = GetCoverURL(item)
                    });
                    Console.WriteLine("Imported Notion page: " + item.Id);
                }

            }

            if (showStorageCovers)
                return pagesCovers.ToList();

            return pagesCovers.Where(x=> !x.IsAlreadyStoraged).ToList();
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