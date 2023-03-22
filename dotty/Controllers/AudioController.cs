using dotty.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotty.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly ILogger<AudioController> _logger;
        private readonly AudioService _audioService;

        public AudioController(ILogger<AudioController> logger, AudioService audioService)
        {
            _logger = logger;
            _audioService = audioService;
        }

        [HttpGet(Name = "CalculatePoints")]
        public async Task<AudioPoints> CalculatePoints(string url, int scale, int? relativeStart, int? customDuration)
        {
            var points = await this._audioService.CalculatePoints(url, scale, relativeStart, customDuration);
            return points;
        }
    }
}