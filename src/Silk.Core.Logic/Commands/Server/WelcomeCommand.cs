using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using SharpYaml.Serialization;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Logic.Commands.Server
{
    [Category(Categories.Server)]
    public class WelcomeCommand : BaseCommandModule
    {

        // What do you think this is for. //
        private const string BaseFile =
            @"config:
    welcome:
        enabled: false # true or false
        greet_on: member_join # member_join, role_grant, or screen_complete (when they agree to server rules)
        greeting_channel: 0 # Id of the channel to greet them in
        message: """" # Valid substitutions {u} -> Username, {@u} -> User mention, {s} -> Server name
        role_id: 0 # Id of the role to check, if configured.";
        private readonly HttpClient _client;
        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;

        public WelcomeCommand(IMediator mediator, IServiceCacheUpdaterService updater, HttpClient client)
        {
            _mediator = mediator;
            _updater = updater;
            _client = client;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [Description
        ("Welcome message settings! Currently supported substitutions:" +
         "\n`{u}` -> Username, `{@u}` -> Mention, `{s}` -> Server Name")]
        public async Task SetWelcome(CommandContext ctx)
        {
            if (!ctx.Message.Attachments.Any())
            {
                await SetWelcomeNoFileAsync(ctx);
            }
            else
            {
                DiscordAttachment? attachment = ctx.Message.Attachments.FirstOrDefault(a => a.FileName is "config.yaml");
                if (attachment is null)
                {
                    await ctx.RespondAsync("Hmm. I don't see `config.yaml` in those attatchments!");
                }
                else
                {
                    var dict = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                    string content = await _client.GetStringAsync(attachment.Url);
                    string? result = VerifyStructure(new Serializer().Deserialize(content, dict));

                    if (result is not null)
                    {
                        await ctx.RespondAsync(result);
                    }
                    else
                    {
                        try { await ConfigureWelcomeAsync(dict, ctx.Guild.Id, ctx); }
                        catch
                        {
                            await ctx.RespondAsync("Something went wrong while parsing your config! Check the file and try again.");
                            return;
                        }
                        await ctx.TriggerTypingAsync();
                        await ctx.RespondAsync("Alright! Changes should apply immediately! Thank you for choosing Silk! <3");
                    }
                }
            }
        }

        private async Task ConfigureWelcomeAsync(Dictionary<string, Dictionary<string, Dictionary<string, object>>> result, ulong guildId, CommandContext ctx)
        {
            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(guildId));
            Dictionary<string, object> dict = result["config"]["welcome"];

            var enabled = (bool) dict["enabled"];
            var greetOn = dict["greet_on"].ToString();
            var greetingChannel = ulong.Parse(dict["greeting_channel"]?.ToString() ?? "0");
            var message = dict["message"].ToString();
            var roleId = ulong.Parse(dict["role_id"].ToString() ?? "0");

            switch (greetOn!.ToLower())
            {
                case "member_join":
                    config.GreetMembers = enabled;
                    config.GreetOnScreeningComplete = false;
                    config.GreetOnVerificationRole = false;
                    break;
                case "screen_complete":
                    config.GreetMembers = enabled;
                    config.GreetOnScreeningComplete = true;
                    config.GreetOnVerificationRole = false;
                    break;
                case "role_grant":
                    config.GreetMembers = enabled;
                    config.GreetOnScreeningComplete = false;
                    config.GreetOnVerificationRole = true;
                    config.VerificationRole = roleId;
                    break;
            }
            config.GreetingChannel = greetingChannel is 0 ? config.GreetingChannel : greetingChannel;

            await _mediator.Send(new UpdateGuildConfigRequest(guildId)
            {
                GreetMembers = config.GreetMembers,
                GreetOnScreeningComplete = config.GreetOnScreeningComplete,
                GreetOnVerificationRole = config.GreetOnVerificationRole,
                VerificationRoleId = config.VerificationRole,
                GreetingChannelId = config.GreetingChannel,
                GreetingText = message
            });
            _updater.UpdateGuild(guildId);
        }

        private async Task SetWelcomeNoFileAsync(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id)
                .WithContent("We know this is going to look oh so janky, but it's the easiest and 'cleanest' way we can do this!\n"
                             + "Just change this config file, and I'll do the rest :)\n"
                             + "(Just rerun this command with the attatched file <3)");

            builder.WithFile("config.yaml", BaseFile.AsStream());

            await ctx.RespondAsync(builder);
        }

        private string? VerifyStructure(object obj)
        {
            if (obj is not Dictionary<string, Dictionary<string, Dictionary<string, object>>> configDict)
                return "Missing entirety of file!";

            if (!configDict.TryGetValue("config", out Dictionary<string, Dictionary<string, object>>? config))
                return "Mising \"config\" section!";

            if (!config.TryGetValue("welcome", out Dictionary<string, object>? welcome))
                return "Missing \"welcome\" section!";

            var options = new[] {"enabled", "greet_on", "greeting_channel", "message", "role_id"};

            string? missingOption = options.FirstOrDefault(o => !welcome.ContainsKey(o));

            if (missingOption is not null)
                return $"Missing \"{missingOption}\" section!";

            return null;
        }
    }
}