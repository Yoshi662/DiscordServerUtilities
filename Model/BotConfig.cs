using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace DiscordServerUtilities.Model
{
	public class BotConfig
	{
		public string Token { get; private set; }
		public LogLevel LogLevel { get; private set; }
		public ulong AuthorID { get; private set; }
		public ulong LoggingServerID { get; private set; }
		public ulong LoggingChannelID { get; private set; }
		public string DataBasePath { get; private set; }
		public string Version { get; set; }
		public string prefix { get; set; }
		public string debugprefix { get; set; }
	}
}
