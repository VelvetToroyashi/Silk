using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Constants;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server.Configuration
{

    [Experimental]
    [RequireGuild]
    [Group("config")]
    [Aliases("configuration")]
    [Category(Categories.Server)]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Description("Edit configurations the caveman way!\nOr perhaps we just haven't launched the dashboard yet..")]
    public partial class BaseConfigCommand : BaseCommandModule
    {
        private readonly IDatabaseService _db;

        public BaseConfigCommand(IDatabaseService db)
        {
            _db = db;
        }
        
        [GroupCommand]
        public async Task Config(CommandContext ctx)
        {
            GuildConfig config = await _db.GetConfigAsync(ctx.Guild.Id);
            DiscordMessageBuilder mBuilder = new();
            StringBuilder sBuilder = new();
            mBuilder.WithoutMentions().WithReply(ctx.Message.Id);

            sBuilder
                .AppendLine($"Configured settings for {ctx.Guild.Name}!")
                .AppendLine()
                .AppendLine("**General Settings:**")
                .AppendLine()
                .AppendLine("\t:sparkles: \t**Greeting**\t :sparkles:")
                .AppendLine($"Greet members? {(config.GreetMembers ? Emojis.Confirm.ToEmoji() : Emojis.Decline.ToEmoji())}")
                .AppendLine();
            

                if (config.GreetMembers)
                {
                    if (config.GreetOnScreeningComplete)
                        sBuilder.AppendLine("I will greet members when they pass Discord's Membership Screening:tm:!");
                    else if (config.GreetOnVerificationRole && config.VerificationRole is not 0)
                        sBuilder.AppendLine($"I will greet members when they are given <@&{config.VerificationRole}>!");
                    else
                        sBuilder.AppendLine("I will greet members as soon as they join!");
                }
                    
                sBuilder
                    .AppendLine()
                    .AppendLine("\t__Greeting Message:__")
                    .AppendLine(AddGreeting(ctx, config));
                
                mBuilder.WithContent(sBuilder.ToString());
            await ctx.RespondAsync(mBuilder);
        }

        private static string AddGreeting(CommandContext ctx, GuildConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.GreetingText))
                return "Not configured!";
            
            string[] interp = config.GreetingText
                .Replace("{u}", ctx.Member.Username)
                .Replace("{s}", ctx.Member.Guild.Name)
                .Replace("{@u}", ctx.Member.Mention).Split('\n');
            
            var sb = new StringBuilder();
            foreach (var line in interp) 
                sb.AppendLine($"> {line}");
            
            return sb.ToString();
        }
    }
}