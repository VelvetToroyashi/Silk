using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using MediatR;
using SharpYaml.Serialization;
using Silk.Core.Constants;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Formatter = DSharpPlus.Formatter;

namespace Silk.Core.Commands.Server
{
    [Category(Categories.Server)]
    public class WelcomeCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;

        private const string BaseFile = 
@"config:
  welcome:
    enabled: false
    greet_on: 0
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

        }
        private async Task SetWelcomeWhenNoFile(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id)
                .WithContent("We know this is going to look oh so janky, but it's the easiest and 'cleanest' way we can do this!\n" +
                             "Just change this config file, and I'll do the rest :)");
            
            builder.WithFile("config.yml", BaseFile.AsStream());
            
            var a = new Serializer().Deserialize(BaseFile);

            await ctx.RespondAsync(Formatter.BlockCode(a.ToString(), "yaml"));
            //await ctx.RespondAsync(builder);
        }
    }
}