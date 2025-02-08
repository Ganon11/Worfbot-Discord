using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Worfbot.Weather.WeatherModels;

namespace Worfbot.Weather
{
  public static class API
  {
    const string WEATHER_API_CONFIGURATION_KEY = "API_KEY";

    private static HttpClient GetHttpClient()
    {
      var client = new HttpClient();
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      client.DefaultRequestHeaders.Add("User-Agent", "Worfbot-Discord");
      return client;
    }

    public static async Task<Location> GetLocationFromZip(string zipCode, IConfiguration configuration, Logging.ILogger? logger = null)
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

    public static async Task<Location> GetLocationFromCityName(string cityName, IConfiguration configuration, string? state = null, string? country = null, Logging.ILogger? logger = null)
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
      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, nameof(CheckWeather), $"JSON Dump: {json}"));
      }
      List<Location> locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Location>>(json) ?? throw new Exception("Couldn't get a location!");
      return !locations.Any() ? throw new Exception("Couldn't get a location!") : locations.First();
    }

    public static async Task<WeatherPrediction> CheckWeather(Location location, Units units, IConfiguration configuration, Logging.ILogger? logger = null)
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
  }
}
