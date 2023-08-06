using Microsoft.AspNetCore.Mvc;
using Saturn.BL;
using Saturn.BL.FeatureUtils;

namespace Saturn.API.Controllers
{
    enum LogLevel
    {
        LogInformation,
        LogWarning
    }

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FeaturesController : Controller
    {
        private readonly FeatureHandler _featureHandler;
        private readonly ILogger<FeaturesController> _logger;

        public FeaturesController(ILogger<FeaturesController> logger)
        {
            _logger = logger;
            _featureHandler = VirtualBox.GetInstance().FeatureHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var features = await _featureHandler.GetFeaturesAsync();

            if (features is null)
            {
                _logger.LogWarning(" @API Not found any features on HttpGet features. Call from: Features/GetAll");

                return NotFound();
            }

            _logger.LogInformation(" @API GetAll Features was made successfully. Call from: Features/GetAll");
            return Ok(features);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? featureName)
        {
            var features = await _featureHandler.GetFeaturesAsync();

            if (string.IsNullOrWhiteSpace(featureName))
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API {nameof(featureName)} cannot be null or empty. Call from: Features/Get/featureName...");
            }

            if (features is null)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API Not found feature {featureName} on HttpGet features. Call from: Features/Get/{featureName}");
            }

            var feature = features.FirstOrDefault(f => f.FeatureName == featureName);

            if (feature is null)
            {
                return ReportExceptionToLog(LogLevel.LogInformation, $" @API Feature {featureName} not found in features. Call from: Features/Get/{featureName}");
            }

            _logger.LogInformation($" @API Get Feature {featureName} was made successfully. Call from: Features/GetAll");

            return Ok(features);
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string? featureName)
        {
            if (_featureHandler is null)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API Featurehandler is null. Call from: Features/Run/{featureName}");
            }

            await Task.Run(() =>
            {
                _featureHandler.EnableFeature(featureName);
            });

            return Ok($"Feature {featureName} enabled");
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string? featureName)
        {
            if (_featureHandler is null)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API Featurehandler is null. Call from: Features/Run/{featureName}");
            }

            await Task.Run(() =>
            {
                _featureHandler.DisableFeature(featureName);
            });

            return Ok($"Feature {featureName} disabled");
        }

        [HttpPost]
        public async Task<IActionResult> Run(string? featureName)
        {
            if (_featureHandler is null)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, $" @API Featurehandler is null. Call from: Features/Run/{featureName}");
            }

            string output = await _featureHandler.TryToRunReturnOutput(featureName);

            return Ok(output);
        }

        [HttpPost]
        public async Task<IActionResult> RunAll()
        {
            if (_featureHandler is null)
            {
                return ReportExceptionToLog(LogLevel.LogWarning, "@API Featurehandler is null.Call from: Features/RunAll");
            }
            await _featureHandler.TryToRunAll();

            return Ok("All features ran successfully");
        }

        private NotFoundResult ReportExceptionToLog(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.LogInformation:
                    _logger.LogInformation(message); break;
                case LogLevel.LogWarning:
                    _logger.LogWarning(message); break;
                default:
                    break;
            }

            return NotFound();
        }
    }
}
