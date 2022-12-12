using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Extensions;
using Silk.Shared;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        
        [Command("invites")]
        [Description("Adjust the settings for invite detection.")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<IResult> Invites
        (
            [Option('d', "delete")]
            [Description("Whether to delete non-whitelisted invites.")]
            bool? delete = null,
            
            [Option('a', "aggressive")]
            [Description("Whether to use a more aggressive invite detection algorithm.")]
            bool? aggressive = null,
            
            [Option('s', "scan")]
            [Description("Whether the origin of the invite should be scanned prior to actioning against it. " +
                         "This is necessary if the server does not have a vanity invite.")]
            bool? scanOrigin = null,
            
            [Option('w', "warn")]
            [Description("Whether to warn the user when an invite is detected.")]
            bool? warnOnMatch = null
        )
        {
            if ((delete ?? aggressive ?? scanOrigin ?? warnOnMatch) is null)
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You must specify at least one option.");
            
            await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
            {
                DeleteOnMatchedInvite = delete      ?? default(Optional<bool>),
                UseAggressiveRegex    = aggressive  ?? default(Optional<bool>),
                ScanInvites           = scanOrigin  ?? default(Optional<bool>),
                WarnOnMatchedInvite   = warnOnMatch ?? default(Optional<bool>)
            });
            
            return Result<ReactionResult>.FromSuccess(new(Emojis.ConfirmId));
        }

        
        #region  Invite Whitelist

        [Command("invite-whitelist", "iw")]
        [Description("Control the whitelisting of invites!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<IResult> WhitelistAsync
        (
            [Option('a', "add")]
            [Description("Add one or more invite(s) to the whitelist. Guild IDs can also be specified!")]
            OneOf<string, Snowflake>[] add,
            
            [Option('r', "remove")]
            [Description("Remove one or more invite(s) from the whitelist. Guild IDs can also be specified!")]
            OneOf<string, Snowflake>[] remove,
            
            [Switch('c', "clear")]
            [Description("Clear the whitelist. For convenience, a dump of all current whitelisted invites will be sent to the channel.")]
            bool clear = false,
            
            [Option("active")]
            [Description("Whether the whitelist is active.")]
            bool? active = null
        )
        {
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            
            if (clear)
            {
                if (!config.Invites.Whitelist.Any())
                {
                    await _channels.CreateMessageAsync(_context.GetChannelID(), "There are no invites to clear!");
                    return Result.FromSuccess();
                }

                var inviteString = config.Invites.Whitelist.Select(r => r.VanityURL).Join(" ");

                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value) {AllowedInvites = Array.Empty<InviteEntity>().ToList()});
                
                await _channels.CreateMessageAsync(_context.GetChannelID(), $"Here's a dump of the whitelist prior to clearing! \n{inviteString}");

                return Result.FromSuccess();
            }

            var addedInvites = new List<string>();
            var failedAdds   = new List<string>();
            
            var removedInvites = new List<string>();
            var failedRemoves  = new List<string>();
            
            
            foreach (var added in add)
            {
                if (added.TryPickT0(out var inviteString, out var guildID))
                {
                    if (config.Invites.Whitelist.Any(iv => iv.VanityURL == inviteString))
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(already whitelisted)`",30}");
                        continue;
                    }

                    var inviteResult = await _invites.GetInviteAsync(inviteString);

                    if (!inviteResult.IsDefined(out var inv))
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invalid invite)`",30}");
                        continue;
                    }

                    if (inv.Guild.IsDefined(out var inviteGuild) && inviteGuild.ID.Value == _context.GuildID.Value)
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invite is for this server)`",30}");
                        continue;
                    }

                    if (inv.ExpiresAt.IsDefined())
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invite is temporary)`",30}");
                        continue;
                    }
                    
                    config.Invites.Whitelist.Add(new () { VanityURL = inviteString, GuildId = _context.GuildID.Value});
                    addedInvites.Add($"`{inviteString,-44}`");
                }
                else
                {
                    if (guildID == _context.GuildID.Value)
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(invite is for this server)`",30}");
                        continue;
                    }
                    
                    if (config.Invites.Whitelist.Any(iv => iv.InviteGuildId == guildID))
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(already whitelisted)`",30}");
                        continue;
                    }
                    
                    config.Invites.Whitelist.Add(new() { GuildId = _context.GuildID.Value, InviteGuildId = guildID });
                    
                    addedInvites.Add($"`{guildID,-44}`");
                }
            }

            foreach (var removed in remove)
            {
                if (!config.Invites.Whitelist.Any())
                {
                    failedRemoves.Add("The whitelist is empty!".PadRight(34));
                    break;
                }
                
                if (removed.TryPickT0(out var inviteString, out var guildID))
                {
                    inviteString = Regex.Replace(inviteString, @"(https?:\/\/discord\.gg\/)?(?<invite>[A-z0-9_-]+)", "${invite}");
                    
                    if (config.Invites.Whitelist.All(iv => iv.VanityURL != inviteString))
                    {
                        failedRemoves.Add($"`{inviteString,-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    config.Invites.Whitelist.RemoveAll(iv => iv.VanityURL == inviteString);
                    removedInvites.Add($"`{inviteString,-44}`");
                }
                else
                {
                    if (config.Invites.Whitelist.All(iv => iv.InviteGuildId != guildID))
                    {
                        failedRemoves.Add($"`{guildID.ToString(),-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    config.Invites.Whitelist.RemoveAll(iv => iv.InviteGuildId == guildID);
                    removedInvites.Add($"`{guildID,-44}`");
                }
            }
            
            var messageBuilder = new StringBuilder();

            if (addedInvites.Any())
            {
                messageBuilder.AppendLine($"Added {addedInvites.Count} invites to the whitelist:");
                
                foreach (var invite in addedInvites)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }

            if (removedInvites.Any())
            {
                messageBuilder.AppendLine($"Removed {removedInvites.Count} invites from the whitelist:");
                
                foreach (var invite in removedInvites)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }
            
            if (failedAdds.Any())
            {
                messageBuilder.AppendLine($"Failed to add {failedAdds.Count} invites from the whitelist:");
                
                foreach (var invite in failedAdds)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }

            if (failedRemoves.Any())
            {
                messageBuilder.AppendLine($"Failed to remove {failedRemoves.Count} invites to the whitelist:");
                
                foreach (var invite in failedRemoves)
                    messageBuilder.AppendLine(invite);
            }
            
            await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
            {
                AllowedInvites = config.Invites.Whitelist,
                BlacklistInvites = active ?? default
            });

            
            if (messageBuilder.Length > 0)
                return await _channels.CreateMessageAsync(_context.GetChannelID(), messageBuilder.ToString());
            
            
            return Result<ReactionResult>.FromSuccess(new(Emojis.ConfirmId));
        }

        #endregion

    }
}