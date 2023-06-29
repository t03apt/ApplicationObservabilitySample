using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Diagnostics;
using WebApi.Diagnostics;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return CreateForecast();
        }

        private static IEnumerable<WeatherForecast> CreateForecast()
        {
            using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Creating forecast");
            activity?.SetTag("custom-tag", "custom-value");

            try
            {
                var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();

                activity?.AddEvent(
                    new ActivityEvent(
                        "Forecast created",
                        tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>> {
                            new("forecast.count", forecast.Length)})));

                foreach (var item in forecast)
                {
                    DiagnosticsConfig.ForecastTemperature.Record(item.TemperatureC);
                }

                return forecast;
            }
            catch (Exception ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.RecordException(ex, new TagList
                {
                    //
                });

                throw;
            }
        }
    }
}