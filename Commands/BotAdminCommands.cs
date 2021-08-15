using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordServerUtilities.Commands
{
	public class BotAdminCommands : BaseCommandModule
	{
		[Command("version"), Description("ShowsBotVersion"), Aliases("v")]
		public async Task Version(CommandContext ctx, [Description("Shows even more things")] bool showdebuginfo = false)
		{
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder
				.WithTitle("ServerUtilites " + Program.botConfig.Version)
				.AddField("Ping:", ctx.Client.Ping + "ms", true)
				.WithColor(Utilities.GetCurrentBotColor(ctx));

			await ctx.RespondAsync(embedBuilder.Build());
		}
	}
}
