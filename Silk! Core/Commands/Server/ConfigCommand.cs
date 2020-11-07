using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Microsoft.EntityFrameworkCore;
using SilkBot.Extensions;
using SilkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SilkBot.Commands.Server
{
    public class ConfigCommand : BaseCommandModule
    {

        public HttpClient Client { private get; set; }
        public IDbContextFactory<SilkDbContext> DbFactory { private get; set; }

        private readonly string ATTACH_CONFIG = "Please attach a config to your message!";
        private readonly string CONFIG_ATTACHED = "Thank you~ I'll let you know if this config is valid, and adjust your settings accordingly!";

        [Command("submit-config")]
        public async Task SubmitConfigCommand(CommandContext ctx)
        {
            DiscordMessage msg = ctx.Message;
            bool containsConfig = msg.Attachments.Any(a => string.Equals(a.FileName, "config.ini", StringComparison.OrdinalIgnoreCase));
            string response = string.Empty;


            await ctx.RespondAsync(containsConfig ? CONFIG_ATTACHED : ATTACH_CONFIG);
            if (containsConfig)
                await ValidateConfigurationAsync(ctx, msg.Attachments.First(a => a.FileName.Equals("config.ini", StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<GuildModel> ValidateConfigurationAsync(CommandContext ctx, DiscordAttachment config)
        {
            string configString = await Client.GetStringAsync(config.Url);
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
                await ctx.RespondAsync($"There was an issue parsing your config! Line: {pe.LineNumber}/{configString.Split('\n').Length} __`{pe.LineValue}`__");
                throw pe;
            }
            catch (ArgumentException ae)
            {
                await ctx.RespondAsync(ae.Message);
                throw ae;
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
            if (!bool.TryParse(serverToggles["WHITELIST_INVITES"], out var WHITELIST_INVITES))
                throw new ArgumentException("Value of `WHITELIST_INVITES` was not true or false.");
            if (!bool.TryParse(serverToggles["ENABLE_BLACKLIST"], out var ENABLE_BLACKLIST))
                throw new ArgumentException("Value of `ENABLE_BLACKLIST` was not true or false.");
            if (!bool.TryParse(serverToggles["LOG_EDITS"], out var LOG_EDITS))
                throw new ArgumentException("Value of `LOG_EDITS` was not true or false.");
            if (!bool.TryParse(serverToggles["GREET_MEMBERS"], out var GREET_MEMBERS))
                throw new ArgumentException("Value of `GREET_MEMBERS` was not true or false.");
            #endregion

            #region Parse roles, if any.
            if (roleInfo["MUTE_ROLE"].Equals("Auto", StringComparison.OrdinalIgnoreCase) && !ulong.TryParse(roleInfo["MUTE_ROLE"], out var MUTE_ROLE))
                throw new ArgumentException("Value of `MUTE_ROLE` was not a valid role Id.");
            if (roleInfo["NO_MEME_ROLE"].Equals("Auto", StringComparison.OrdinalIgnoreCase) && !ulong.TryParse(roleInfo["NO_MEME_ROLE"], out var NO_MEME_ROLE))
                throw new ArgumentException("Value of `NO_MEME_ROLE` was not a valid role Id.");
            #endregion

            #region Server info parsing
            string INFRACTION_FORMAT = serverInfo["INFRACTION_FORMAT"].Equals("Auto", StringComparison.OrdinalIgnoreCase) ? string.Empty : serverInfo["INFRACTION_FORMAT"];
            List<string> WHITELISTED_LINKS = serverInfo["WHITELISTED_LINKS"].Equals("Auto", StringComparison.OrdinalIgnoreCase) ? null : serverInfo["WHITELISTED_LINKS"].Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> BLACKLISTED_WORDS = serverInfo["BLACKLISTED_WORDS"].Equals("Auto", StringComparison.OrdinalIgnoreCase) ? null : serverInfo["BLACKLISTED_WORDS"].Split('"').WhereMoreThan(3).Select(t => t.Replace("\"", null)).ToList();
            List<ulong> SELF_ASSIGN_ROLES = new List<ulong>();
            serverInfo["SELF_ASSIGN_ROLES"]
                .Split(new[] { ',', ' ' }).ToList()
                .ForEach(n =>
                    {
                        if (n.Equals("Auto", StringComparison.OrdinalIgnoreCase)) return;
                        if (n.Length < 18) throw new ArgumentException($"Value of `{n}` was too short to be a snowflake Id.");
                        else if (!ulong.TryParse(n, out var id)) throw new ArgumentException($"Value of `{id}` was not a valid snowflake.");
                        else SELF_ASSIGN_ROLES.Add(id);
                    });
            ulong.TryParse(serverInfo["GP_LOG_CHANNEL_ID"], out var GP_LOG_CHANNEL);
            #endregion

            using var db = DbFactory.CreateDbContext();
            GuildModel guild = db.Guilds.First(g => g.DiscordGuildId == guildId);
            guild.WhitelistInvites = WHITELIST_INVITES;
            guild.WhiteListedLinks = WHITELISTED_LINKS.Select(l => new WhiteListedLink() { Link = l }).Distinct().ToList();
            guild.BlacklistWords = ENABLE_BLACKLIST;
            guild.BlackListedWords = BLACKLISTED_WORDS.Select(w => new BlackListedWord { Word = w }).Distinct().ToList();
            guild.GreetMembers = GREET_MEMBERS;


        }
    }
}
