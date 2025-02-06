using Newtonsoft.Json;

#pragma warning disable CS8618
public class Weather
{
  [JsonProperty("id")]
  public int Id { get;set; }

  [JsonProperty("main")]
  public string Main { get; set; }

  [JsonProperty("description")]
  public string Description { get; set; }
}
#pragma warning restore CS8618
