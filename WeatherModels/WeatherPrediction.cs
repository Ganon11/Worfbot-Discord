using Newtonsoft.Json;

namespace Worfbot.WeatherModels
{
#pragma warning disable CS8618
  public class WeatherPrediction
  {
    [JsonProperty("weather")]
    public Weather[] WeatherForecasts { get; set; }

    [JsonProperty("main")]
    public Temperatures Temps { get; set; }

    [JsonProperty("wind")]
    public Wind Wind { get; set; }

    [JsonProperty("name")]
    public string Location { get; set; }
  }
#pragma warning restore CS8618
}
