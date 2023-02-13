using System.ComponentModel;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    //TODO: Infraction config (stepped, not named)
    
    [Group("edit", "e")]
    [Description("Edit the settings for your server.")]
    public partial class EditConfigCommands : CommandGroup
    {
        private readonly IMediator              _mediator;
        private readonly ITextCommandContext         _context;
        private readonly IDiscordRestGuildAPI   _guilds;
        private readonly IDiscordRestUserAPI    _users;
        private readonly IDiscordRestInviteAPI  _invites;
        private readonly IDiscordRestChannelAPI _channels;
        private readonly IDiscordRestWebhookAPI _webhooks;
        public EditConfigCommands
        (
            IMediator              mediator,
            ITextCommandContext         context,
            IDiscordRestGuildAPI   guilds,
            IDiscordRestUserAPI    users,
            IDiscordRestChannelAPI channels,
            IDiscordRestInviteAPI  invites,
            IDiscordRestWebhookAPI webhooks
        )
        {
            _mediator      = mediator;
            _context       = context;
            _guilds        = guilds;
            _users         = users;
            _channels      = channels;
            _invites       = invites;
            _webhooks = webhooks;
        }
    }

}