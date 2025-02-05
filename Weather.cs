using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Ganon11.Worfbot
{
  public static class WeatherUtilities
  {
    const string WEATHER_API_CONFIGURATION_KEY = "WEATHER_API_KEY";

    public static async Task<WeatherPrediction> CheckWeather(string zipCode, IConfiguration configuration)
    {
      string apiKey = configuration[WEATHER_API_CONFIGURATION_KEY];
      using (HttpClient client = new())
      {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "Worfbot-Discord");

        var json = await client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?zip={zipCode}&appid={apiKey}&units=imperial");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherPrediction>(json);
      }
    }
  }
}