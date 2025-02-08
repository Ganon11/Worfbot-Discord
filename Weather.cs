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

    public static async Task<Location> GetLocationFromZip(string zipCode, IConfiguration configuration, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY] ?? throw new Exception("No API KEY setup!");
      using HttpClient client = GetHttpClient();

      var uri = new UriBuilder("http://api.openweathermap.org/geo/1.0/zip");
      var parameters = HttpUtility.ParseQueryString(string.Empty);
      parameters["zip"] = zipCode;

      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(GetLocationFromZip), $"Making API call to {uri.ToString()}"));
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(GetLocationFromZip), $"Parameters: {parameters.ToString()}"));
      }

      parameters["appid"] = apiKey;

      uri.Query = parameters.ToString();

      string json = await client.GetStringAsync(uri.Uri);
      Location location = Newtonsoft.Json.JsonConvert.DeserializeObject<Location>(json) ?? throw new Exception("Couldn't get a location!");
      return location;
    }

    public static async Task<Location> GetLocationFromCityName(string cityName, IConfiguration configuration, string? state = null, string? country = null, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY] ?? throw new Exception("No API KEY setup!");
      using HttpClient client = GetHttpClient();

      var uri = new UriBuilder("http://api.openweathermap.org/geo/1.0/direct");
      var parameters = HttpUtility.ParseQueryString(string.Empty);

      StringBuilder q = new();
      q.Append(cityName);

      if (state != null)
      {
        q.Append($",{state}");
      }

      if (country != null)
      {
        q.Append($",{country}");
      }

      parameters["q"] = q.ToString();
      parameters["limit"] = "1";

      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(GetLocationFromCityName), $"Making API call to {uri.ToString()}"));
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(GetLocationFromCityName), $"Parameters: {parameters.ToString()}"));
      }

      parameters["appid"] = apiKey;

      uri.Query = parameters.ToString();

      string json = await client.GetStringAsync(uri.Uri);
      List<Location> locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Location>>(json) ?? throw new Exception("Couldn't get a location!");
      return !locations.Any() ? throw new Exception("Couldn't get a location!") : locations.First();
    }

    public static async Task<WeatherPrediction> CheckWeather(Location location, Units units, IConfiguration configuration, ILogger? logger = null)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY] ?? throw new Exception("No API KEY setup!");
      using HttpClient client = GetHttpClient();

      var uri = new UriBuilder("https://api.openweathermap.org/data/2.5/weather");
      var parameters = HttpUtility.ParseQueryString(string.Empty);
      parameters["lat"] = location.Latitude.ToString();
      parameters["lon"] = location.Longitude.ToString();
      parameters["units"] = units.ToString().ToLower();

      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(CheckWeather), $"Making API call to {uri.ToString()}"));
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(CheckWeather), $"Parameters: {parameters.ToString()}"));
      }

      parameters["appid"] = apiKey;

      uri.Query = parameters.ToString();

      string json = await client.GetStringAsync(uri.Uri);
      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, nameof(CheckWeather), $"JSON Dump: {json}"));
      }
      WeatherPrediction weatherPrediction = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherPrediction>(json) ?? throw new Exception("Couldn't get a weather prediction!");
      return weatherPrediction;
    }

    private static string FormatDegrees(double degrees, Units units)
    {
      return units switch
      {
        Units.Standard => $"{degrees} K",
        Units.Imperial => $"{degrees}°F",
        Units.Metric => $"{degrees}°C",
        _ => $"{degrees}°F",
      };
    }

    private static string FormatTemperatures(Temperatures temps, Units units)
    {
      return $"{FormatDegrees(temps.Temperature, units)} (between {FormatDegrees(temps.MinTemp, units)} and {FormatDegrees(temps.MaxTemp, units)}), 🍃 Feels Like {FormatDegrees(temps.FeelsLike, units)}";
    }

    private static string FormatWindSpeed(double speed, Units units)
    {
      return units == Units.Metric ? $"{speed} m/s" : $"{speed} mph";
    }

    private static string FormatWindDirection(double degrees)
    {
      if (degrees < 0 || degrees > 360)
      {
        throw new Exception("Invalid degrees!");
      }

      if (degrees < 11.25)
      {
        return "⬆️ North";
      }

      if (degrees < 33.75)
      {
        return "⬆️ North-Northeast";
      }

      if (degrees < 56.25)
      {
        return "↗️ Northeast";
      }

      if (degrees < 78.75)
      {
        return "↗️ East-Northeast";
      }

      if (degrees < 101.25)
      {
        return "➡️ East";
      }

      if (degrees < 123.75)
      {
        return "➡️ East-Southeast";
      }

      if (degrees < 146.25)
      {
        return "↘️ Southeast";
      }

      if (degrees < 168.75)
      {
        return "↘️ South-Southeast";
      }

      if (degrees < 191.25)
      {
        return "⬇️ South";
      }

      if (degrees < 213.75)
      {
        return "⬇️ South-Southwest";
      }

      if (degrees < 236.25)
      {
        return "↙️ Southwest";
      }

      if (degrees < 258.75)
      {
        return "↙️ West-Southwest";
      }

      if (degrees < 281.25)
      {
        return "⬅️ West";
      }

      if (degrees < 303.75)
      {
        return "⬅️ West-Northwest";
      }

      if (degrees < 326.25)
      {
        return "↖️ Northwest";
      }

      if (degrees < 348.75)
      {
        return "↖️ North-Northwest";
      }

      return "⬆️ North";
    }

    private static string FormatWind(Wind wind, Units units)
    {
      return $"{FormatWindSpeed(wind.WindSpeed, units)} {FormatWindDirection(wind.Degrees)}, gusts of {FormatWindSpeed(wind.Gust, units)}";
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
      stringBuilder.AppendLine($"🌡️ {FormatTemperatures(prediction.Temps, units)}");
      stringBuilder.AppendLine($"💨 {FormatWind(prediction.Wind, units)}");
      return stringBuilder.ToString();
    }
  }
}
