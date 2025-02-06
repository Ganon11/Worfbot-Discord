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
      switch (id)
      {
        case 800:
          return "😎";
        case 801:
          return "🌤️";
        case 802:
          return "⛅";
        case 803:
        case 804:
        default:
          return "☁️";
      }
    }

    public static string FormatWeatherPrediction(WeatherPrediction prediction, Units units)
    {
      StringBuilder stringBuilder = new();
      stringBuilder.AppendLine($"{GetWeatherEmoji(prediction.WeatherForecasts.First().Id)} {prediction.WeatherForecasts.First().Main}");
      stringBuilder.AppendLine($"🌡️ {FormatDegrees(prediction.Temps.Temperature, units)}");
      stringBuilder.Append($"⬆️ High of {FormatDegrees(prediction.Temps.High, units)}, ");
      stringBuilder.Append($"⬇️ Low of {FormatDegrees(prediction.Temps.Low, units)}, ");
      stringBuilder.Append($"🍃 Feels Like {FormatDegrees(prediction.Temps.FeelsLike, units)}");
      return stringBuilder.ToString();
    }
  }
}
