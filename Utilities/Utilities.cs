using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordServerUtilities
{
	public static class Utilities
	{
		/// <summary>
		/// Reccommended max Size for ProgressBars inside of Embeds
		/// </summary>
		const int PB_MAX_SIZE = 18;

		/// <summary>
		/// Creates a <see cref="DiscordEmbedBuilder"/> with some common ocurring params
		/// </summary>
		/// <param name="title">Title of the embed</param>
		/// <param name="descripcion">Descripcion of the embed</param>
		/// <param name="footerText">Footer Text of the embed</param>
		/// <param name="color">Color of the embed, Defaults to <see cref="DiscordColor.None"/></param>
		/// <returns>A <see cref="DiscordEmbedBuilder"/> with the proper parameters</returns>
		public static DiscordEmbedBuilder QuickEmbed(string title, string descripcion, string footerText = "", DiscordColor color = new())
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			{
				Title = title,
				Description = descripcion,
				Color = color
			};
			builder.WithFooter(footerText);
			return builder;
		}

		/// <summary>
		/// Creates a <see cref="DiscordEmbedBuilder"/> with some common ocurring params
		/// </summary>
		/// <param name="ctx">CommandContext to get the current Bot Color, for more info <seealso cref="GetCurrentBotColor(CommandContext)"/></param>
		/// <param name="title">Title of the embed</param>
		/// <param name="descripcion">Descripcion of the embed</param>
		/// <param name="footerText">Footer Text of the embed</param>
		/// <returns>A <see cref="DiscordEmbedBuilder"/> with the proper parameters and the colour of the higest role</returns>
		public static DiscordEmbedBuilder QuickEmbed(CommandContext ctx, string title, string descripcion, string footerText = "")
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
			{
				Title = title,
				Description = descripcion,
				Color = GetCurrentBotColor(ctx)
			};
			builder.WithFooter(footerText);
			return builder;
		}
		/// <summary>
		/// Gets the color of the highest role the bot has right now.
		/// </summary>
		public static DiscordColor GetCurrentBotColor(CommandContext ctx)
		{
			//From the context we get the guild. From then we get the BotMember. Then we got their roles and from then we got the color of the first role
			var roles = ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).Result
							.Roles.OrderBy(o => o.Position)
								.ToList();
			DiscordColor color = DiscordColor.None;
			foreach (var item in roles)
			{
				if (item.Color.Value != 0) color = item.Color;
			}
			return color;
		}

		/// <summary>
		/// Gets a progress bar
		/// </summary>
		/// <param name="percentage">Number between 0 and 1 represntating the progress</param>
		/// <param name="max_size">The number of characters you want the progress bar to be</param>
		/// <returns></returns>
		public static string GenerateProgressBar(double percentage, int max_size = PB_MAX_SIZE)
		{
			if (percentage > 1 || percentage < 0)
				throw new ArgumentOutOfRangeException("Percentaje must be between 0 and 1");

			double completed_PB = percentage * max_size;
			return $"*{new string('▰', (int)completed_PB) + new string('▱', max_size - (int)completed_PB)}*{$" - **[{percentage:P}]**"}";
		}

		/// <summary>
		///	Synchronously sleeps the current <see cref="System.Threading.Thread"/>
		/// </summary>
		/// <param name="ms">Time of the delay in miliseconds</param>
		public static void Delay(int ms = 350) => System.Threading.Thread.Sleep(ms);
	}
}
