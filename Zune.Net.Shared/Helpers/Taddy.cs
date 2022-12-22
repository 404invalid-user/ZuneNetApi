﻿using Flurl.Http;
using Newtonsoft.Json.Linq;
using OwlCore.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Zune.Xml.Catalog;

namespace Zune.Net.Helpers
{
    public static partial class Taddy
    {
        public const string API_BASE = "https://api.taddy.org";
        public static readonly string[] DEFAULT_PODCAST_PROPS = new[] { "uuid", "name", "description(shouldStripHtmlTags: true)",
            "rssUrl", "websiteUrl", "imageUrl", "genres", "authorName", "isExplicitContent", "datePublished" };

        public static IFlurlRequest GetBase()
        {
            return API_BASE
                .WithHeader("Content-Type", "application/json")
                .WithHeader("X-USER-ID", Constants.TD_USER_ID)
                .WithHeader("X-API-KEY", Constants.TD_API_KEY);
        }

        public static async Task<TaddyPodcastSeries> GetMinimalPodcastInfo(string name)
        {
            RequestData request = new($"{{ getPodcastSeries(name: \"{name}\") {{ uuid rssUrl description(shouldStripHtmlTags: true) }} }}");

            var response = await GetBase().PostJsonAsync(request);
            var responseObj = await response.GetJsonAsync<JToken>();

            var data = responseObj["data"]["getPodcastSeries"];
            return new(
                Guid.Parse(data.Value<string>("uuid")),
                data.Value<string>("rssUrl"),
                data.Value<string>("description")
            );
        }

        public static async Task<PodcastSeries> GetPodcastByTDID(Guid tdid, params string[] props)
        {
            if (props == null || props.Length == 0)
                props = DEFAULT_PODCAST_PROPS;

            RequestData request = new($"{{ getPodcastSeries(uuid: \"{tdid}\") {{ {string.Join(' ', props)} }} }}");

            var response = await GetBase().PostJsonAsync(request);
            var responseObj = await response.GetJsonAsync<JToken>();

            var data = responseObj["data"]["getPodcastSeries"];
            PodcastSeries podcast = new()
            {
                Id = tdid.ToString()
            };

            podcast.Title = data.Value<string>("name");
            podcast.Content = data.Value<string>("description");
            podcast.WebsiteUrl = data.Value<string>("websiteUrl");
            podcast.FeedUrl = data.Value<string>("rssUrl");

            podcast.Author = new()
            {
                Name = data.Value<string>("authorName"),
            };

            var datePublished = data.Value<int?>("datePublished");
            if (datePublished != null)
                podcast.ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(datePublished.Value).LocalDateTime;

            var imageUrl = data.Value<string>("imageUrl");
            if (imageUrl != null)
            {
                podcast.ImageUrl = imageUrl;
                podcast.Images = new()
                {
                    new()
                    {
                        Instances = new ImageInstance
                        {
                            Url = imageUrl,
                        }.IntoList()
                    }
                };
            }

            var genreIds = data["genres"].Values<string>();
            if (genreIds != null)
            {
                podcast.Categories = genreIds.Select(id =>
                    new Category
                    {
                        Id = id,
                        Title = Genres[id]
                    }).ToList();
            }

            var isExplicit = data.Value<bool?>("isExplicitContent");
            if (isExplicit != null)
                podcast.Explicit = isExplicit.Value;

            return podcast;
        }

        private record RequestData(string query);

        public record TaddyPodcastSeries(Guid Id, string RssUrl, string Description);
    }
}
