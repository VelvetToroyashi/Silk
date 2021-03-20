using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using SharpYaml.Serialization;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Commands.Server
{
    [Category(Categories.Server)]
    public class WelcomeCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;

        // What do you think this is for. //
        private const string BaseFile = 
                @"config:
                welcome:
                    enabled: false
                    greet_on: member_join
                    greeting_channel: 0
                    message: """"
                    role_id: 0";
        
        public WelcomeCommand(IMediator mediator, IServiceCacheUpdaterService updater)
        {
            _mediator = mediator;
            _updater = updater;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [Description
        ("Welcome message settings! Currently supported substitutio5they ns:" +
         "\n`{u}` -> Username, `{@u}` -> Mention, `{s}` -> Server Name")]
        public async Task SetWelcome(CommandContext ctx)
        {
            if (!ctx.Message.Attachments.Any())
            {
                await SetWelcomeWhenNoFile(ctx);
                return;
            }
            VerifyStructure(new Serializer().Deserialize(new Serializer().Serialize(BaseFile)));
        }
        private async Task SetWelcomeWhenNoFile(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id)
                .WithContent("We know this is going to look oh so janky, but it's the easiest and 'cleanest' way we can do this!\n"
                             + "Just change this config file, and I'll do the rest :)\n"
                             + "(Just rerun this command with the attatched file <3)");
            
            builder.WithFile("config.yml", BaseFile.AsStream());

            

            var dict = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

                        
            new Serializer().Deserialize(BaseFile, dict);
            
            await ctx.RespondAsync(builder);
        }

        private string? VerifyStructure(object obj)
        {
            if (obj is not Dictionary<string, object> config)
                return "Missing entirety of file!";
            
            if (!config.TryGetValue("config", out object? welcome))
                return "Mising \"config\" section!";
            
            if (welcome is not Dictionary<string, object> welcomeOptions)
                return "Missing \"welcome\" section!";
            
            var options = new[] { "enabled", "greet_on", "greeting_channel", "message", "role_id" };
            
            string? missingOption = options.FirstOrDefault(o => !welcomeOptions.ContainsKey(o));
            
            if (missingOption is not null)
                return $"Missing \"{missingOption}\" section!";
            
            return null;
        }
    }
}