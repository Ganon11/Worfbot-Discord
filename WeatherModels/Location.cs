using Newtonsoft.Json;

namespace Worfbot.WeatherModels
{
#pragma warning disable CS8618
  public class Location
  {
    [JsonProperty("zip")]
    public string ZipCode { get; set; }

    [JsonProperty("name")]
    public string LocationName { get; set; }

    [JsonProperty("lat")]
    public double Latitude { get; set; }

    [JsonProperty("lon")]
    public double Longitude { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
  }
#pragma warning restore CS8618
}
