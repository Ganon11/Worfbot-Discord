using Microsoft.Extensions.Configuration;

namespace Ganon11.Worfbot
{
  public static class HonorUtilities
  {
    private static string CreateMD5(string input)
    {
      using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
      {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
      }
    }

    private static readonly char[] HONORABLE_SUFFIXES = new char[] { '0', '1', '2', '3', '4', '5', '6', '7' };

    public static async Task<bool> DetermineHonor(string topic, IConfiguration configuration, ILogger? logger = null)
    {
      var database = new WorfbotDatabase(configuration);
      var databaseStatus = await database.DetermineHonorFromDatabase(topic);
      if (databaseStatus != HonorStatus.Unknown)
      {
        if (logger != null)
        {
          await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(DetermineHonor), $"Honor determined from database"));
        }
        return databaseStatus == HonorStatus.Honorable;
      }

      var md5 = CreateMD5(topic);
      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(DetermineHonor), $"Honor determined from MD5 hash ({md5})"));
      }
      return HONORABLE_SUFFIXES.Contains(md5.Last());
    }

    public static async Task SetHonor(string topic, bool status, IConfiguration configuration)
    {
      var database = new WorfbotDatabase(configuration);
      await database.SetHonorInDatabase(topic, status);
    }
  }
}