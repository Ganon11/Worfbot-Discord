using Newtonsoft.Json;

namespace Worfbot.WeatherModels
{
#pragma warning disable CS8618
  public class Temperatures
  {
    [JsonProperty("temp")]
    public double Temperature { get; set;}

    [JsonProperty("feels_like")]

    public double FeelsLike { get; set;}

    [JsonProperty("temp_min")]
    public double Low { get; set;}

    [JsonProperty("temp_max")]
    public double High { get; set;}
  }
#pragma warning restore CS8618
}
