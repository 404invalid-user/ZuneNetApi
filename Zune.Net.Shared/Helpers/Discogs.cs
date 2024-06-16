using System;
using Flurl.Http;

namespace Zune.Net.Helpers
{
    public static partial class Discogs
    {
        public const string API_BASE = "https://api.discogs.com";
        public static readonly string DC_API_KEY = Environment.GetEnvironmentVariable("DC_API_KEY");
        public static readonly string DC_API_SECRET = Environment.GetEnvironmentVariable("DC_API_SECRET");

        public static IFlurlRequest WithAuth(string url)
        {
            return url.WithHeader("User-Agent", Constants.USERAGENT_ZUNE48_GITHUB)
                      .WithHeader("Authorization", $"Discogs key={DC_API_KEY}, secret={DC_API_SECRET}");
        }
    }
}
