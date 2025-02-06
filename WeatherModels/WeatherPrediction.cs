using Newtonsoft.Json;

#pragma warning disable CS8618
public class WeatherPrediction
{
  [JsonProperty("weather")]
  public Weather[] WeatherForecasts { get; set; }

  [JsonProperty("main")]
  public Temperatures Temps { get; set; }

  [JsonProperty("name")]
  public string Location { get; set; }
}
#pragma warning restore CS8618
