using JikanDotNet;
using malAnimeUpdater;
using Microsoft.AspNetCore.Mvc;
using Tautulli.MAL.Updater.Models;

namespace Tautulli.MAL.Updater.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpdateController : ControllerBase
    {
        private readonly MALService _malService;

        private readonly ILogger<UpdateController> _logger;

        public UpdateController(ILogger<UpdateController> logger, MALService malService)
        {
            _logger = logger;
            _malService = malService;
        }

        [HttpPost(Name = "SetEpisode")]
        public async Task<IActionResult> SetEpisode(Episode episode)
        {
            var id = await _malService.GetAnimeId(episode.Name, episode.Date, episode.TmdbId);

            if (id == null)
            {
                _logger.LogWarning($"Anime not found: {episode.Name} ({id}) episode: {episode.EpisodeNumber} airing date: {episode.Date}");
                return Ok("Anime not found");
            }
            _logger.LogInformation($"Anime found: {episode.Name} ({id}) episode: {episode.EpisodeNumber} airing date: {episode.Date}");
            var result = await _malService.SetWatchedEpisodeTo((long)id, episode.EpisodeNumber);
            _logger.LogInformation($"Update status: {result}");

            return Ok();
        }
    }
}