using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Microsoft.EntityFrameworkCore;
using SilkBot.Database;
using SilkBot.Database.Models;
using SilkBot.Extensions;
using SilkBot.Models;
using SilkBot.Utilities;

namespace SilkBot.Commands.Server
{
    // TODO: Clean up this monstrosity //
    [Category(Categories.Server)]
    [Group("config")]
    public class ConfigCommand : BaseCommandModule
    {
        private readonly HttpClient _client;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public ConfigCommand(HttpClient client, IDbContextFactory<SilkDbContext> factory)
        {
            _client = client;
            _dbFactory = factory;
        }

        private readonly string ATTACH_CONFIG = "Please attach a config to your message!";

        private readonly string CONFIG_ATTACHED =
            "Thank you~ I'll let you know if this config is valid, and adjust your settings accordingly!";


        [GroupCommand]
        public async Task GetConfig(CommandContext ctx)
        {
            await ctx.RespondWithFileAsync("./config.ini",
                $"Want to submit a config? Sure! Just edit this file, and return it with `{ctx.Prefix}config submit`! :)" +
                $"You can edit individual settings via `{ctx.Prefix}config edit <section>`. To see all sections, run `{ctx.Prefix}config list-sections`");
        }


        [Command("submit")]
        public async Task SubmitConfigCommand(CommandContext ctx)
        {
            DiscordMessage msg = ctx.Message;
            bool containsConfig = msg.Attachments.Any(a =>
                string.Equals(a.FileName, "config.ini", StringComparison.OrdinalIgnoreCase));
            var response = string.Empty;


            await ctx.RespondAsync(containsConfig ? CONFIG_ATTACHED : ATTACH_CONFIG);
            if (containsConfig)
                await ValidateConfigurationAsync(ctx,
                    msg.Attachments.First(a => a.FileName.Equals("config.ini", StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<GuildModel> ValidateConfigurationAsync(CommandContext ctx, DiscordAttachment config)
        {
            string configString = await _client.GetStringAsync(config.Url);
            var parserConfig = new IniDataParser(new IniParserConfiguration
            {
                CommentString = "//",
                CaseInsensitive = false,
                ThrowExceptionsOnError = true
            });
            var parser = new StreamIniDataParser(parserConfig);
            try
            {
                ParseConfig(parser, configString, ctx.Guild.Id);
            }
            catch (ParsingException pe)
            {
                await ctx.RespondAsync(
                    $"There was an issue parsing your config! Line: {pe.LineNumber}/{configString.Split('\n').Length} __`{pe.LineValue}`__");
                throw;
            }
            catch (ArgumentException ae)
            {
                await ctx.RespondAsync(ae.Message);
                throw;
            }

            // Commands are internally wrapped in Try/Catch, so re-throwing allows us to return from the method entirely instead of checking the status of the object. //
            return new GuildModel();
        }

        private void ParseConfig(StreamIniDataParser parser, string configString, ulong guildId)
        {
            IniData data = parser.Parser.Parse(configString);
            KeyDataCollection serverToggles = data["SERVER_TOGGLES"];
            KeyDataCollection roleInfo = data["ROLE_INFO"];
            KeyDataCollection serverInfo = data["SERVER_INFO"];

            #region Parse server toggles

            if (!bool.TryParse(serverToggles["WHITELIST_INVITES"], out bool WHITELIST_INVITES))
                throw new ArgumentException("Value of `WHITELIST_INVITES` was not true or false.");
            if (!bool.TryParse(serverToggles["ENABLE_BLACKLIST"], out bool ENABLE_BLACKLIST))
                throw new ArgumentException("Value of `ENABLE_BLACKLIST` was not true or false.");
            if (!bool.TryParse(serverToggles["LOG_EDITS"], out bool LOG_EDITS))
                throw new ArgumentException("Value of `LOG_EDITS` was not true or false.");
            if (!bool.TryParse(serverToggles["GREET_MEMBERS"], out bool GREET_MEMBERS))
                throw new ArgumentException("Value of `GREET_MEMBERS` was not true or false.");

            #endregion

            #region Parse roles, if any.

            if (roleInfo["MUTE_ROLE"].Equals("Auto", StringComparison.OrdinalIgnoreCase) &&
                !ulong.TryParse(roleInfo["MUTE_ROLE"], out ulong MUTE_ROLE))
                throw new ArgumentException("Value of `MUTE_ROLE` was not a valid role Id.");
            if (roleInfo["NO_MEME_ROLE"].Equals("Auto", StringComparison.OrdinalIgnoreCase) &&
                !ulong.TryParse(roleInfo["NO_MEME_ROLE"], out ulong NO_MEME_ROLE))
                throw new ArgumentException("Value of `NO_MEME_ROLE` was not a valid role Id.");

            #endregion

            #region Server info parsing

            string INFRACTION_FORMAT =
                serverInfo["INFRACTION_FORMAT"].Equals("Auto", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : serverInfo["INFRACTION_FORMAT"];
            List<string> WHITELISTED_LINKS =
                serverInfo["WHITELISTED_LINKS"].Equals("Auto", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : serverInfo["WHITELISTED_LINKS"].Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries)
                                                     .ToList();
            List<string> BLACKLISTED_WORDS =
                serverInfo["BLACKLISTED_WORDS"].Equals("Auto", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : serverInfo["BLACKLISTED_WORDS"].Split('"').WhereMoreThan(3).Select(t => t.Replace("\"", null))
                                                     .ToList();
            var SELF_ASSIGN_ROLES = new List<ulong>();
            serverInfo["SELF_ASSIGN_ROLES"]
                .Split(new[] {',', ' '}).ToList()
                .ForEach(n =>
                {
                    if (n.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return;
                    if (n.Length < 18)
                        throw new ArgumentException($"Value of `{n}` was too short to be a snowflake Id.");
                    else if (!ulong.TryParse(n, out ulong id))
                        throw new ArgumentException($"Value of `{id}` was not a valid snowflake.");
                    else SELF_ASSIGN_ROLES.Add(id);
                });
            ulong.TryParse(serverInfo["GP_LOG_CHANNEL_ID"], out ulong GP_LOG_CHANNEL);

            #endregion

            using SilkDbContext db = _dbFactory.CreateDbContext();
            GuildModel guild = db.Guilds.First(g => g.Id == guildId);
            guild.WhitelistInvites = WHITELIST_INVITES;
            guild.WhiteListedLinks =
                WHITELISTED_LINKS.Select(l => new WhiteListedLink() {Link = l}).Distinct().ToList();
            guild.BlacklistWords = ENABLE_BLACKLIST;
            guild.BlackListedWords = BLACKLISTED_WORDS.Select(w => new BlackListedWord {Word = w}).Distinct().ToList();
            guild.GreetMembers = GREET_MEMBERS;
        }

        [Command("List-Sections")]
        [Aliases("List")]
        [RequireFlag(UserFlag.Staff)]
        public async Task ListSections(CommandContext ctx)
        {
            // Becuase of the way Android renders embeds for some reason, quoted codeblocks are broken. Dunno. //
            await ctx.RespondAsync($">>> ```md\n" +
                                   $"Available sections:\n" +
                                   $"\t- WhitelistInvites <true/false>\n" +
                                   $"\t- Auto-Dehoist <PREMIUM> <true/false>\n" +
                                   $"\t- LogMessages <true/false>\n" +
                                   $"\t- MuteRole <UID>\n" +
                                   $"\t- MessageLogChannel <UID>\n" +
                                   $"\t- ModLogChannel <UID> (Default: MessageLogChannel)\n```");
        }

        [Category(Categories.Server)]
        [Group("Edit")]
        public class ConfigEditCommand : BaseCommandModule
        {
            private readonly IDbContextFactory<SilkDbContext> _dbFactory;

            public ConfigEditCommand(IDbContextFactory<SilkDbContext> dbFactory)
            {
                _dbFactory = dbFactory;
            }

            [GroupCommand]
            public async Task Edit(CommandContext ctx)
            {
                await ctx.RespondAsync("Please provide a section you'd like to edit, and it's value!");
            }

            [Command("WhitelistInvites")]
            [RequireFlag(UserFlag.Staff)]
            public async Task WhitelistInvites(CommandContext ctx, bool whitelist)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.WhitelistInvites = whitelist;
                await AssertConfigAsync<bool>(ctx, guild.WhitelistInvites, whitelist);
                await db.SaveChangesAsync();
            }

            [Command("Auto-Dehoist")]
            [RequireFlag(UserFlag.Staff)]
            [RequireFlag(UserFlag.SilkPremiumUser)]
            public async Task AutoDehoist(CommandContext ctx, bool dehoist)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.AutoDehoist = dehoist;
                await AssertConfigAsync<bool>(ctx, guild.AutoDehoist, dehoist);
                await db.SaveChangesAsync();
            }

            [Command("LogMessages")]
            [RequireFlag(UserFlag.Staff)]
            public async Task LogMessages(CommandContext ctx, bool logMessages)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.LogMessageChanges = logMessages;
                await AssertConfigAsync<bool>(ctx, guild.LogMessageChanges, logMessages);
                await db.SaveChangesAsync();
            }

            [Command("MuteRole")]
            [RequireFlag(UserFlag.Staff)]
            public async Task Mute(CommandContext ctx, DiscordRole role)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.MuteRoleId = role.Id;
                await AssertConfigAsync<ulong>(ctx, guild.MuteRoleId, role.Id);
                await db.SaveChangesAsync();
            }

            [Command("MessageLogChannel")]
            [RequireFlag(UserFlag.Staff)]
            public async Task MLog(CommandContext ctx, DiscordChannel channel)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.MessageEditChannel = channel.Id;
                if (guild.GeneralLoggingChannel == default) guild.GeneralLoggingChannel = channel.Id;
                await AssertConfigAsync<ulong>(ctx, guild.MessageEditChannel, channel.Id);
                await db.SaveChangesAsync();
            }

            [Command("ModlogChannel")]
            [RequireFlag(UserFlag.Staff)]
            public async Task Modlog(CommandContext ctx, DiscordChannel channel)
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
                guild.GeneralLoggingChannel = channel.Id;
                await AssertConfigAsync<ulong>(ctx, guild.GeneralLoggingChannel, channel.Id);
                await db.SaveChangesAsync();
            }


            private async Task AssertConfigAsync<T>(CommandContext ctx, object value, object expectedValue)
            {
                if (((T) value).Equals((T) expectedValue))
                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                else
                    await ctx.RespondAsync("Something went wrong when applying that setting.");
            }
        }
    }
}