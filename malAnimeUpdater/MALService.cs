using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JikanDotNet;
using Polly;
using TMDbLib.Client;
using TMDbLib.Objects.TvShows;

namespace malAnimeUpdater;

public class MALService
{
    private readonly TMDbClient _client;
    private readonly HttpClient _httpClient;
    private readonly IJikan _jikan;

    public MALService(string MalhlogSessionId, string MalSessionId, string tmdbApiKey)
    {
        _client = new TMDbClient(tmdbApiKey);
        _jikan = new Jikan();

        var cookies = new CookieContainer();

        cookies.Add(new CookieCollection
        {
            new Cookie("is_logged_in", "1", "/", "myanimelist.net"),
            new Cookie("MALHLOGSESSID", MalhlogSessionId, "/", "myanimelist.net"),
            new Cookie("MALSESSIONID", MalSessionId, "/", "myanimelist.net")
        });
        var httpClientHandler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = true,
            CookieContainer = cookies
        };
        _httpClient = new HttpClient(httpClientHandler);
    }


    public async Task<long?> GetAnimeId(string title, string date, string tmdbId)
    {
        var policy = Policy<long?>
            .Handle<Exception>()
            .FallbackAsync(async cancellationToken =>
            {
                var shows = await GetShowsByTitle(await GetAlternativeTitle(tmdbId));
                return FilterByAiredDate(date, shows) != null ? FilterByAiredDate(date, shows).MalId : null;
            });

        var id = await policy.ExecuteAsync(async () =>
        {
            var shows = await GetShowsByTitle(title);
            if (shows.Count == 0) throw new Exception("Anime not found");
            var id = FilterByAiredDate(date, shows) != null ? FilterByAiredDate(date, shows).MalId : throw new Exception("Anime not found by date");
            return id;
        });

        return id;
    }

    private async Task<string> GetAlternativeTitle(string tmdbId)
    {
        var tmdbAnime = await _client.GetTvShowAsync(int.Parse(tmdbId), TvShowMethods.AlternativeTitles);
        var alternativeTitle = tmdbAnime.AlternativeTitles.Results.FirstOrDefault(_ => _.Iso_3166_1 == "JP").Title;

        return alternativeTitle;
    }

    private static Anime FilterByAiredDate(string date, IEnumerable<Anime> animes)
    {
        var anime = animes.Where(_ => _.Aired.From.HasValue).FirstOrDefault(_ =>
        {
            var airingDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var year = _.Aired.From.Value.Year;
            return year == airingDate.Year && _.Aired.From.Value.Month == airingDate.Month;
        });
        return anime;
    }

    private async Task<List<Anime>> GetShowsByTitle(string title)
    {
        var result = new DbPaginatedJikanResponse<ICollection<Anime>>();
        var shows = new List<Anime>();
        do
        {
            result = await _jikan.SearchAnimeAsync(new AnimeSearchConfig
            {
                Query = title,
                Type = AnimeType.TV,
                OrderBy = AnimeSearchOrderBy.StartDate,
                Page = result.Pagination != null ? result.Pagination.CurrentPage + 1 : 1,
            });
            shows.AddRange(result.Data);
            await Task.Delay(1000);
        } while (result.Pagination.HasNextPage);

        return shows;
    }

    public async Task<bool> SetWatchedEpisodeTo(long animeId, int count)
    {
        var csrfToken = await GetCsrfToken();

        var result = await SetEpisode(animeId, count, csrfToken);
        return result.IsSuccessStatusCode && result.StatusCode == HttpStatusCode.OK;
    }

    private async Task<HttpResponseMessage> SetEpisode(long animeId, int count, string csrfToken)
    {
        var json =
            $"{{\"anime_id\":{animeId},\"status\":1,\"score\":0,\"num_watched_episodes\":{count},\"csrf_token\":\"{csrfToken}\"}}";

        var result = await _httpClient.PostAsync("https://myanimelist.net/ownlist/anime/edit.json",
            new StringContent(json));
        return result;
    }

    private async Task<string> GetCsrfToken()
    {
        var response = await _httpClient.GetAsync("https://myanimelist.net");
        var html = new HtmlDocument();
        html.LoadHtml(await response.Content.ReadAsStringAsync());
        var csrfToken = html.DocumentNode.Descendants("meta")
            .FirstOrDefault(_ => _.GetAttributeValue("name", "").Contains("csrf_token"))
            ?.GetAttributeValue("content", "");
        return csrfToken;
    }
}