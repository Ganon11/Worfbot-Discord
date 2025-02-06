using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Worfbot.WeatherModels;

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

    private static HttpClient GetHttpClient()
    {
      var client = new HttpClient();
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      client.DefaultRequestHeaders.Add("User-Agent", "Worfbot-Discord");
      return client;
    }

    public static async Task<Location> GetLatitudeLongitudeFromZip(string zipCode, IConfiguration configuration, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY] ?? throw new Exception("No API KEY setup!");
      using HttpClient client = GetHttpClient();

      var uri = new UriBuilder("http://api.openweathermap.org/geo/1.0/zip");
      var parameters = HttpUtility.ParseQueryString(string.Empty);
      parameters["zip"] = zipCode;

      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(GetLatitudeLongitudeFromZip), $"Making API call to {uri.ToString()}"));
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(CheckWeather), $"Parameters: {parameters.ToString()}"));
      }

      parameters["appid"] = apiKey;

      uri.Query = parameters.ToString();

      string json = await client.GetStringAsync(uri.Uri);
      Location location = Newtonsoft.Json.JsonConvert.DeserializeObject<Location>(json) ?? throw new Exception("Couldn't get a location!");
      return location;
    }

    public static async Task<WeatherPrediction> CheckWeather(string zipCode, Units units, IConfiguration configuration, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY] ?? throw new Exception("No API KEY setup!");
      using HttpClient client = GetHttpClient();

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
      WeatherPrediction weatherPrediction = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherPrediction>(json) ?? throw new Exception("Couldn't get a weather prediction!");
      return weatherPrediction;
    }

    private static string FormatDegrees(double degrees, Units units)
    {
      switch (units)
      {
        case Units.Standard:
          return $"{degrees} K";
        case Units.Imperial:
          return $"{degrees}°F";
        case Units.Metric:
          return $"{degrees}°C";
        default:
          return $"{degrees}°F";
      }
    }

    private static string GetWeatherEmoji(int id)
    {
      // Thunderstorm
      if (200 <= id && id <= 232)
      {
        return "⛈️";
      }

      // Drizzle
      if (300 <= id && id <= 321)
      {
        return "☔";
      }

      // Rain
      if (500 <= id && id <= 531)
      {
        return "🌧️";
      }

      // Snow
      if (600 <= id && id <= 622)
      {
        return "🌨️";
      }

      // Atmosphere (???)
      if (700 <= id && id < 781)
      {
        return "🌫️";
      }

      // Clear, or Clouds
      return id switch
      {
        800 => "😎",
        801 => "🌤️",
        802 => "⛅",
        _ => "☁️",
      };
    }

    private static string GetWeatherBriefSummary(WeatherPrediction prediction)
    {
      List<string> individualConditions = new();
      foreach (Weather weather in prediction.WeatherForecasts)
      {
        individualConditions.Add($"{GetWeatherEmoji(weather.Id)} {weather.Main}");
      }

      return string.Join(", ", individualConditions);
    }

    public static string FormatWeatherPrediction(WeatherPrediction prediction, Units units)
    {
      StringBuilder stringBuilder = new();
      stringBuilder.AppendLine($"{GetWeatherBriefSummary(prediction)}");
      stringBuilder.AppendLine($"🌡️ {FormatDegrees(prediction.Temps.Temperature, units)}");
      stringBuilder.Append($"⬆️ High of {FormatDegrees(prediction.Temps.High, units)}, ");
      stringBuilder.Append($"⬇️ Low of {FormatDegrees(prediction.Temps.Low, units)}, ");
      stringBuilder.Append($"🍃 Feels Like {FormatDegrees(prediction.Temps.FeelsLike, units)}");
      return stringBuilder.ToString();
    }
  }
}
