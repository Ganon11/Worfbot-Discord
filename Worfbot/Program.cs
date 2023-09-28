using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Npgsql;

public class Program
{
   public Program()
   {
      _client = new DiscordSocketClient();
   }

   private DiscordSocketClient _client;

   public static Task Main(string[] args) => new Program().MainAsync();

   public async Task MainAsync()
   {
      _client.Log += Log;
      _client.Ready += Client_Ready;
      _client.SlashCommandExecuted += SlashCommandHandler;

      string token;
      if (File.Exists("token.txt"))
      {
         token = File.ReadAllText("token.txt");
      }
      else
      {
         var secret = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
         if (secret == null)
         {
            System.Console.Error.WriteLine("Could not find bot token");
            return;
         }

         token = secret;
      }

      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      await Task.Delay(-1);
   }

   private Task Log(LogMessage msg)
   {
      Console.WriteLine(msg.ToString());
      return Task.CompletedTask;
   }

   private async Task Client_Ready()
   {
      var globalCommand = new SlashCommandBuilder()
         .WithName("honor")
         .WithDescription("Checks whether the given topic is honorable.")
         .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to determine", isRequired: true);

      try {
         await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
      }
      catch (HttpException ex)
      {
         var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
         Console.WriteLine(json);
      }
   }

   private async Task SlashCommandHandler(SocketSlashCommand command)
   {
      switch (command.Data.Name)
      {
         case "honor":
            await HandleHonorCommand(command);
            break;
      }
   }

   private static string CreateMD5(string input)
   {
      using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
      {
         byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
         byte[] hashBytes = md5.ComputeHash(inputBytes);

         return Convert.ToHexString(hashBytes);
      }
   }

   private static char[] HONORABLE_SUFFIXES = new char[] { '0', '1', '2', '3', '4', '5', '6', '7' };

   public enum HonorStatus
   {
      Dishonorable = 0,
      Honorable = 1,
      Unknown = 2
   }

   private static async Task<HonorStatus> CheckDatabase(string term)
   {
      var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
      var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
      var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
      var port = Environment.GetEnvironmentVariable("DATABASE_PORT");
      var name = Environment.GetEnvironmentVariable("DATABASE_NAME");

      var connString = $"Host={host};Username={username};Password={password};Database={name};SSL Mode=Disable";

      await using var conn = new NpgsqlConnection(connString);
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand("SELECT status FROM honorable WHERE topic = ($1)", conn)
      {
         Parameters =
         {
            new() { Value = term }
         }
      };

      var result = await cmd.ExecuteScalarAsync();
      if (result == null)
      {
         return HonorStatus.Unknown;
      }

      return (bool)result ? HonorStatus.Honorable : HonorStatus.Dishonorable;
   }

   private static async Task<bool> DetermineHonor(string term)
   {
      var databaseStatus = await CheckDatabase(term);
      if (databaseStatus != HonorStatus.Unknown)
      {
         return databaseStatus == HonorStatus.Honorable;
      }

      var md5 = CreateMD5(term);
      if (HONORABLE_SUFFIXES.Contains(md5.Last()))
      {
         return true;
      }

      return false;
   }

   private async Task HandleHonorCommand(SocketSlashCommand command)
   {
      var option = command.Data.Options.FirstOrDefault();
      if (option == default)
      {
         return;
      }

      var topic = option.Value.ToString();
      if (topic == null)
      {
         return;
      }

      Console.WriteLine($"{command.User.Username} requested honor status of topic \"{topic}\"");
      var honor = await DetermineHonor(topic);
      if (honor)
      {
         await command.RespondAsync($"{topic} has honor.");
      }
      else
      {
         await command.RespondAsync($"{topic} is without honor.");
      }

      return;
   }
}
