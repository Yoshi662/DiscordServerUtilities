using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.CommandsNext.Attributes;
using DiscordServerUtilities.Model;
using DSharpPlus.Net;
using System.IO;
using DiscordServerUtilities.Commands;

namespace DiscordServerUtilities
{
	public class Program
	{
		/* BIG TODO LIST
		 * Add commands
		 * Improve Version command
		 */

		public static void Main(string[] args)
		{
			prog = new Program();
			prog.RunBotAsync().GetAwaiter().GetResult();
		}

		private static Program prog;
		public static DiscordClient client;
		public static BotConfig botConfig;

		CommandsNextExtension commands;
		CommandsNextConfiguration commandsconfig;

		DiscordChannel loggerChannel;
		DiscordUser botAuthor;

		public async Task RunBotAsync()
		{
			DiscordConfiguration Config = new()
			{
				Token = botConfig.Token,
				AutoReconnect = true,
				MinimumLogLevel = botConfig.LogLevel,
				Intents = DiscordIntents.All, //TODO Change depending to usage
			};

			client = new(Config);

			client.Ready += GeneralInformationLogger;
			client.GuildAvailable += GeneralInformationLogger;
			client.ClientErrored += ClientFailed;

			client.UseInteractivity(new InteractivityConfiguration
			{
				PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
				Timeout = TimeSpan.FromMinutes(2)
			});

			commandsconfig = new()
			{
				StringPrefixes = new[] { System.Diagnostics.Debugger.IsAttached ? "sud!" : "su!" },
				DmHelp = true,
				IgnoreExtraArguments = true
			};

			commands = client.UseCommandsNext(commandsconfig);

			commands.RegisterCommands<BotAdminCommands>();
			commands.RegisterCommands<ModerationCommands>();
			commands.RegisterCommands<UtilityCommands>();

			commands.CommandExecuted += CommandsLogger;
			commands.CommandErrored += CommandsLogger;

			//Connection

			Utilities.Delay(1000);

			await client.ConnectAsync();
			await client.UpdateStatusAsync(new DiscordActivity("Prefix = [su!]", ActivityType.Playing));

			botAuthor = await client.GetUserAsync(botConfig.AuthorID);
			loggerChannel = await client.GetChannelAsync(botConfig.LoggingChannelID);

		}

		private Task GeneralInformationLogger(DiscordClient sender, EventArgs e)
		{
			string messagelog = e switch
			{
				ReadyEventArgs => "Client Inicialised",
				GuildCreateEventArgs => $"Guild Detected [{(e as GuildCreateEventArgs).Guild.Name}]",
				null => throw new ArgumentNullException(nameof(e), $"{e.GetType().Name} cannot be null"),
				_ => throw new ArgumentException(nameof(e), $"{e.GetType().Name} Is not supported on this method"),
			};
			sender.Logger.LogInformation(messagelog);
			return Task.CompletedTask;
		}
		private Task ClientFailed(DiscordClient sender, ClientErrorEventArgs e)
		{
			sender.Logger.Log(LogLevel.Error, $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
			prog.RunBotAsync().GetAwaiter().GetResult();
			return Task.CompletedTask;
		}

		private Task CommandsLogger(CommandsNextExtension ctx, EventArgs e)
		{
			switch (e)
			{
				case CommandExecutionEventArgs:
					CommandExecutionEventArgs exEA = (CommandExecutionEventArgs)e;
					exEA.Context.Client.Logger.LogInformation($"{exEA.Context.User.Username} successfully executed '{exEA.Command.QualifiedName}'");
					break;
				case CommandErrorEventArgs:
					CommandErrorEventArgs errEA = (CommandErrorEventArgs)e;
					errEA.Context.Client.Logger.LogError($"{errEA.Context.User.Username} tried executing '{errEA.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {errEA.Exception.GetType()}: {errEA.Exception.Message ?? "<no message>"}", DateTime.Now);

					// let's check if the error is a result of lack of required permissions

					DiscordEmbed exEmbed = new DiscordEmbedBuilder().Build();

					//if we're debugging always show the full stack
					switch (System.Diagnostics.Debugger.IsAttached ? "" : errEA.Exception.GetType().Name)
					{
						case "UnauthorizedException":
							exEmbed = Utilities.QuickEmbed(errEA.Context, "Access denied", ":no_entry: You do not have the permissions required to execute this command.");
							break;
						case "ArgumentException":
							exEmbed = Utilities.QuickEmbed(errEA.Context, "Incorrect Arguments", $":no_entry: You have entered an incorrect or invalid number of parameters, Type `am!help {errEA.Command.Name}` to get more info about the command.");
							break;
						case "ChecksFailedException":
							//TODO Improve this part of code
							ChecksFailedException checksFailedException = (ChecksFailedException)errEA.Exception;
							if (checksFailedException.FailedChecks.Count == 1)
							{
								exEmbed = checksFailedException.FailedChecks[0].GetType().Name switch
								{
									"CooldownAttribute" => Utilities.QuickEmbed(errEA.Context, "Command is in cooldown", $":hourglass_flowing_sand: {(checksFailedException.FailedChecks[0] as CooldownAttribute).GetRemainingCooldown(errEA.Context).Seconds} Seconds left.\nPlease wait."),
									"RequirePermissionsAttribute" => Utilities.QuickEmbed(errEA.Context, "Failed Permissions", ":no_entry: You don't have enough permissions to execute this command"),
									"RequireUserPermissionsAttribute" => Utilities.QuickEmbed(errEA.Context, "Failed Permissions", ":no_entry: You don't have enough permissions to execute this command"),
									"RequireOwnerAttribute" => Utilities.QuickEmbed(errEA.Context, "Failed Permissions", ":no_entry: Only the owner of this bot can use this command"),
									_ => Utilities.QuickEmbed(errEA.Context, "Requeriments Failed", checksFailedException.FailedChecks[0].GetType().Name),
								};
							} else
							{
								string checks = "";
								foreach (CheckBaseAttribute item in checksFailedException.FailedChecks)
								{
									checks += item.GetType().Name + "\n";
								}
								exEmbed = Utilities.QuickEmbed(errEA.Context, "Requeriments Failed", checks);
							}
							break;

						//This prevents a error message from happening when a command is not found which is a common occurrance
						case "CommandNotFoundException":
							break;
						default:
							DiscordEmbedBuilder exEmbedBuilder = new();
							exEmbedBuilder
								.WithTitle(":warning: Something Happened :warning:")
								.WithColor(new DiscordColor("#FF0000")
								);

							if (errEA.Exception.HelpLink != null) exEmbedBuilder.WithUrl(errEA.Exception.HelpLink);
							if (errEA.Exception.Message != null) exEmbedBuilder.AddField("Mensaje", errEA.Exception.Message);
							if (errEA.Command?.QualifiedName != null) exEmbedBuilder.AddField("Commando", errEA.Command?.QualifiedName);
							if (errEA.Exception.GetType().Name != null) exEmbedBuilder.AddField("Type", errEA.Exception.GetType().Name);
							if (errEA.Exception.StackTrace != null) exEmbedBuilder.AddField("StackTrace", (errEA.Exception.StackTrace.Length > 1000 ? errEA.Exception.StackTrace.Substring(0, 1000) : errEA.Exception.StackTrace));
							exEmbedBuilder.WithFooter("For Debug purposes only");
							exEmbedBuilder.WithTimestamp(DateTime.Now);
							exEmbed = exEmbedBuilder;
							break;
					}
					//This prevents sending empty messages
					if (!String.IsNullOrWhiteSpace(exEmbed.Title))
					{
						errEA.Context.RespondAsync(exEmbed);
					}
					break;

				default:
					throw new ArgumentException(nameof(e), $"{e.GetType().Name} Is not supported on this method");
			}
			return Task.CompletedTask;
		}
	}
}
