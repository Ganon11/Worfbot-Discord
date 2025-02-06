using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Ganon11.Worfbot
{
  public static class WeatherUtilities
  {
    public enum Units
    {
      None = 0,
      Standard = 1,
      Imperial = 2,
      Metric = 3
    }

    const string WEATHER_API_CONFIGURATION_KEY = "API_KEY";

    public static async Task<WeatherPrediction> CheckWeather(string zipCode, Units units, IConfiguration configuration, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY];
      using (HttpClient client = new())
      {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "Worfbot-Discord");

        var uri = new UriBuilder("https://api.openweathermap.org/data/2.5/weather");
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["zip"] = zipCode;
        parameters["units"] = units.ToString().ToLower();

        if (logger != null)
        {
          await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(CheckWeather), $"Making API call to {uri.ToString()}"));
          await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(CheckWeather), $"Parameters: {parameters.ToString()}"));
        }

        parameters["appid"] = apiKey;

        uri.Query = parameters.ToString();

        string json = await client.GetStringAsync(uri.Uri);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherPrediction>(json);
      }
    }

    private static string FormatDegrees(double degrees, Units units)
    {
      switch (units)
      {
        case Units.Standard:
          return $"{degrees} K";
        case Units.Imperial:
          return $"{degrees}° F";
        case Units.Metric:
          return $"{degrees}° C";
        default:
          return $"{degrees}° F";
      }
    }

    public static string FormatWeatherPrediction(WeatherPrediction prediction, Units units)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"{prediction.weather.First().main}");
      stringBuilder.AppendLine($"{FormatDegrees(prediction.main.temp, units)}");
      stringBuilder.Append($"⬆️ High of {FormatDegrees(prediction.main.temp_max, units)}, ");
      stringBuilder.Append($"⬇️ Low of {FormatDegrees(prediction.main.temp_min, units)}, ");
      stringBuilder.Append($"🍃 Feels Like {FormatDegrees(prediction.main.feels_like, units)})");
      return stringBuilder.ToString();
    }
  }
}