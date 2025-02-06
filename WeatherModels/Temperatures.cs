using Newtonsoft.Json;

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
