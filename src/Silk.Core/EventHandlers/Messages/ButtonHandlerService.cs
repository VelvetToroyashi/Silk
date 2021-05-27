using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Services;

namespace Silk.Core.EventHandlers.Messages
{
    public class ButtonHandlerService
    {
        private readonly ILogger<ButtonHandlerService> _logger;
        private readonly ConfigService _config;
        public ButtonHandlerService(ILogger<ButtonHandlerService> logger, ConfigService config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task OnButtonPress(DiscordClient client, ComponentInteractionCreateEventArgs args)
        {
            if (args.Id.StartsWith("rolemenu assign ", StringComparison.OrdinalIgnoreCase))
            {
                try { await args.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate); }
                catch
                {
                    /* We'll still try to assign/unassign the role. */
                }

                GuildConfig config = await _config.GetConfigAsync(args.Guild.Id);
                if (config.RoleMenus.All(r => r.MessageId != args.Message.Id))
                    return; // Not a valid role menu anymore. //

                ulong roleId = ulong.Parse(args.Id.Split(' ')[2]);
                DiscordRole? role = args.Guild.GetRole(roleId);

                if (role is null)
                {
                    await args.Interaction.CreateFollowupMessageAsync(new() {Content = "Sorry, but it seems that role doesn't exist anymore!", IsEphemeral = true});
                    return;
                }

                if (role.Position >= args.Guild.CurrentMember.Hierarchy)
                {
                    await args.Interaction.CreateFollowupMessageAsync(new() {Content = "Sorry, but that role was moved above mine, and thus I can't assign it!", IsEphemeral = true});
                    return;
                }

                DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id); // They may not be in cache. //

                if (member.Roles.Contains(role))
                {
                    await member.RevokeRoleAsync(role);
                    await args.Interaction.CreateFollowupMessageAsync(new() {Content = $"Done! You no longer have {role.Mention}.", IsEphemeral = true});
                }
                else
                {
                    await member.RevokeRoleAsync(role);
                    await args.Interaction.CreateFollowupMessageAsync(new() {Content = $"Done! You now have {role.Mention}.", IsEphemeral = true});
                }
            }
        }
    }
}