using DSharpPlus.Entities;
using IniParser.Exceptions;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class INIConfigHander
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public INIConfigHander(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;
        public async Task<GuildModel> ParseINIAsync(string rawConfig, DiscordChannel channel, CancellationToken token = default)
        {
            var config = new IniParserConfiguration() { ThrowExceptionsOnError = true, CaseInsensitive = false, CommentString = "//",  };
            var parser = new IniDataParser(config);
            try 
            {
                IniData INI = parser.Parse(rawConfig);
                var db = _dbFactory.CreateDbContext();
                GuildModel guild = await db.Guilds.FirstOrDefaultAsync(g => g.DiscordGuildId == channel.Guild.Id, token) ?? new GuildModel();

                KeyDataCollection channels = INI["ID_VALUES"];
                // Assign channels. //
                guild.GreetingChannel       = ParseUlong(INI, "ID_VALUES", "GREETING_CHANNEL");
                guild.GeneralLoggingChannel = ParseUlong(INI, "ID_VALUES", "LOGGING_CHANNEL");
                guild.MessageEditChannel    = ParseUlong(INI, "ID_VALUES", "EDIT_CHANNEL");

                // Assign values. //

                guild.GreetMembers  = ParseBool(INI, "SERVER", "GREET_MEMBERS");
                guild.LogMessageChanges     = ParseBool(INI, "SERVER", "LOG_MSG_EDIT");
                guild.WhitelistInvites      = ParseBool(INI, "SERVER", "WHITELIST_INVITES");
                guild.BlacklistWords        = ParseBool(INI, "SERVER", "ENABLE_BLACKLIST");
                guild.BlackListedWords      = INI["SERVER"]["BLACKLIST"] == "NULL" ? 
                                                                            new List<BlackListedWord>() : 
                                                                            INI["SERVER"]["BLACKLIST"].Split(',').Select(w => new BlackListedWord() { Word = w }).ToList();

            }
            catch (ParsingException e) 
            {
                await channel.SendMessageAsync($"There was a problem parsing the config file you've passed me: `{e.Message}`");
                throw e;
            }
            catch (ArgumentException e)
            {
                await channel.SendMessageAsync($"Your config has an issue! {e.ParamName} was not a valid input");
                throw e;
            }
            return null;
        }

        public ulong ParseUlong(IniData INI, string section, string key) => ulong.Parse(INI[section][key]);
        public bool ParseBool(IniData INI, string section, string key) => bool.Parse(INI[section][key]);

        private void ValidateInputs(IniData data, CancellationToken token)
        {
            foreach (KeyData value in data["ID_VALUES"])
                if (!ulong.TryParse(value.Value, out _))
                    throw new ArgumentException(nameof(value.Value));
        }

    }
}
