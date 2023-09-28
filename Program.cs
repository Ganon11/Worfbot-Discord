using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Inflector;
using Newtonsoft.Json;
using Npgsql;

namespace Ganon11.Worfbot
{
   public class Program
   {
      private readonly IConfiguration _configuration;
      private readonly IServiceProvider _serviceProvider;

      public Program()
      {
         _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DATABASE_")
            .AddEnvironmentVariables(prefix: "DISCORD_BOT_")
            .Build();

         var collection = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton<DiscordSocketClient>();

         _serviceProvider = collection.BuildServiceProvider();
      }

      public static Task Main(string[] args) => new Program().MainAsync();

      public async Task MainAsync()
      {
         var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

         client.Log += Log;
         client.Ready += Client_Ready;
         client.SlashCommandExecuted += SlashCommandHandler;

         await client.LoginAsync(TokenType.Bot, _configuration["TOKEN"]);
         await client.StartAsync();

         await Task.Delay(-1);
      }

      private Task Log(LogMessage msg)
      {
         Console.WriteLine(msg.ToString());
         return Task.CompletedTask;
      }

      private async Task Client_Ready()
      {
         var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

         var honorCommand = new SlashCommandBuilder()
            .WithName("honor")
            .WithDescription("Checks whether the given topic is honorable.")
            .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to determine", isRequired: true);

         var setHonorCommand = new SlashCommandBuilder()
            .WithName("set-honor")
            .WithDescription("Informs Worfbot of the honorability of the topic.")
            .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to set", isRequired: true)
            .AddOption("status", ApplicationCommandOptionType.Boolean, "True if the topic is honorable, false otherwise", isRequired: true);

         try
         {
            await client.CreateGlobalApplicationCommandAsync(honorCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(setHonorCommand.Build());
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
            case "set-honor":
               await HandleSetHonorCommand(command);
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

      private static NpgsqlConnection ConnectToDatabase()
      {
         var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
         var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
         var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
         var port = Environment.GetEnvironmentVariable("DATABASE_PORT");
         var name = Environment.GetEnvironmentVariable("DATABASE_NAME");

         var connString = $"Host={host};Username={username};Password={password};Database={name};SSL Mode=Disable";

         var conn = new NpgsqlConnection(connString);

         return conn;
      }

      private static async Task<HonorStatus> DetermineHonorFromDatabase(string topic)
      {
         await using var conn = ConnectToDatabase();
         await conn.OpenAsync();

         await using var selectCommand = new NpgsqlCommand("SELECT status FROM honorable WHERE topic = ($1)", conn)
         {
            Parameters =
            {
               new() { Value = topic }
            }
         };

         var result = await selectCommand.ExecuteScalarAsync();
         if (result == null)
         {
            return HonorStatus.Unknown;
         }

         return (bool)result ? HonorStatus.Honorable : HonorStatus.Dishonorable;
      }

      private static async Task SetHonorInDatabase(string topic, bool status)
      {
         await using var conn = ConnectToDatabase();
         await conn.OpenAsync();

         await using var selectCommand = new NpgsqlCommand("SELECT status FROM honorable WHERE topic = ($1)", conn)
         {
            Parameters = 
            {
               new() { Value = topic }
            }
         };

         var result = await selectCommand.ExecuteScalarAsync();
         if (result == null)
         {
            // Insert new row
            await using var insertCommand = new NpgsqlCommand("INSERT INTO honorable(topic, status) VALUES (($1), ($2))", conn)
            {
               Parameters =
               {
                  new() { Value = topic },
                  new() { Value = status }
               }
            };

            await insertCommand.ExecuteNonQueryAsync();
         }
         else
         {
            // Update existing row
            await using var updateCommand = new NpgsqlCommand("UPDATE honorable SET status = ($2) WHERE topic = ($1)", conn)
            {
               Parameters =
               {
                  new() { Value = topic },
                  new() { Value = status }
               }
            };

            await updateCommand.ExecuteNonQueryAsync();
         }
      }

      private static async Task<bool> DetermineHonor(string topic)
      {
         var databaseStatus = await DetermineHonorFromDatabase(topic);
         if (databaseStatus != HonorStatus.Unknown)
         {
            return databaseStatus == HonorStatus.Honorable;
         }

         var md5 = CreateMD5(topic);
         if (HONORABLE_SUFFIXES.Contains(md5.Last()))
         {
            return true;
         }

         return false;
      }

      private bool IsPlural(string topic)
      {
         Console.WriteLine($"Checking pluralization of {topic}");
         var inflector = new Inflector.Inflector(new CultureInfo("en"));
         var pluralizedTopic = inflector.Pluralize(topic);

         Console.WriteLine($"{topic} pluralized is {pluralizedTopic}");

         return topic.Equals(pluralizedTopic, StringComparison.CurrentCultureIgnoreCase);
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
            var verb = IsPlural(topic) ? "have" : "has";
            await command.RespondAsync($"{topic} {verb} honor.");
         }
         else
         {
            var verb = IsPlural(topic) ? "are" : "is";
            await command.RespondAsync($"{topic} {verb} without honor.");
         }

         return;
      }

      private async Task HandleSetHonorCommand(SocketSlashCommand command)
      {
         var topicOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("topic"));
         if (topicOption == null)
         {
            return;
         }

         var statusOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("status"));
         if (statusOption == null)
         {
            return;
         }

         var topic = topicOption.Value.ToString();
         if (topic == null)
         {
            return;
         }

         var statusString = statusOption.Value.ToString();
         if (statusString == null)
         {
            return;
         }

         if (!bool.TryParse(statusString, out var status))
         {
            return;
         }

         await SetHonorInDatabase(topic, status);

         await command.RespondAsync($"{topic}'s honor status has been set to {status}.", ephemeral: true);
         return;
      }
   }
}
