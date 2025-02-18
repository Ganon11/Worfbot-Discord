using Newtonsoft.Json;

namespace Worfbot.Weather.WeatherModels
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

    [JsonProperty("rain")]
    public Rain? Rain { get; set; }

    [JsonProperty("snow")]
    public Snow? Snow { get; set; }
  }
#pragma warning restore CS8618
}
