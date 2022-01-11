using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace RoleMenuPlugin.Responders;

public class RoleMenuHelpButtonResponder : IResponder<IInteractionCreate>
{
	private const string 
		DiscordTab = "\u200b\t",
	    HelpMessage =
		    "**Role Menu Creator Instructions**\n"                                                                                   +
		    "There are many buttons whom's function may not be entirely clear.\n\n"                                                  +
		    "From left to right, here's an explaination of what each does.\n\n"                                                      +
		    "`Add (Interactive)`:\n"                                                                                                 +
		    $"{DiscordTab}This option is the interactive way of adding roles, but can be tediuous.\n"                                +
		    $"{DiscordTab}Using this button will prompt you for the role, emoji, and a description for the option.\n"                +
		    $"{DiscordTab}For the role, it cannot be `@everyone` nor a role that is higher than either of our roles.\n"              +
		    $"{DiscordTab}For the emoji you can use any emoji, but they must be typed out correctly!\n"                              +
		    $"{DiscordTab}(e.g. <a:catgiggle:853806288190439436> or 👋 and not catgiggle or \\:wave\\:)\n"                           +
		    $"{DiscordTab}Descriptions are also easy. They can be whatever you want, but they will limited to 100 characters.\n\n"   +
		    "`Add (Simple)`:\n"                                                                                                      +
		    $"{DiscordTab}As it's name would imply, this is a simplier, but more restrictive way of adding roles.\n"                 +
		    $"{DiscordTab}You can only add a role, but you can add multiple roles at a time.\n"                                      +
		    $"{DiscordTab}When using this option, you must mention the role directly (e.g. `@role`).\n"                              +
		    $"{DiscordTab}The edit button can be used to retroactively add descriptions and emojis to these options!\n"              +
		    $"{DiscordTab}The same rules apply in regards to `@everyone` and roles that are higher than either of our roles.\n\n"    +
		    "`Edit option`:\n"                                                                                                       +
		    $"{DiscordTab}This button allows you to edit various options of the role menu.\n"                                        +
		    $"{DiscordTab}After selecting the option you'd like to edit, you can perform various actions on it via the buttons.\n"   +
		    "`Help`:\n"                                                                                                              +
		    $"{DiscordTab}Shows this very help menu!"                                                                                +
		    "`Finish`:\n"                                                                                                            +
		    $"{DiscordTab}This is the final button, and will finish the role menu\\*.\n"                                             +
		    $"{DiscordTab}It will send the role menu to the channel you created it in, and will also delete the original message.\n" +
		    $"{DiscordTab}*This button is only available if you have added at least one role to the menu.\n"                         +
		    "`Cancel`:\n"                                                                                                            +
		    $"{DiscordTab}This button will cancel the role menu creation process, cleaning up any residual messages.\n"              +
		    "**Note**:\n"                                                                                                            +
		    $"{DiscordTab}It's recommended to start with the interactive option.\n"                                                  +
		    $"{DiscordTab}Also, this plugin is in a very early stage of development, so the menus may have some bugs.\n"             +
		    $"{DiscordTab}If you find any, don't hesitate to [tell us](https://velvetthepanda.dev/silk)!";
	    

	
    private readonly IDiscordRestInteractionAPI _interactions;
    public RoleMenuHelpButtonResponder(IDiscordRestInteractionAPI interactions) => _interactions = interactions;

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
	    if (!gatewayEvent.Data.IsDefined(out var data)   ||
	        !data.ComponentType.IsDefined(out var cType) ||
	        !data.CustomID.IsDefined(out var cID)        ||
	        cType is not ComponentType.Button            ||
	        cID != "rm-help")
		    return Result.FromSuccess();
	    

	    await _interactions.CreateInteractionResponseAsync(gatewayEvent.ID, gatewayEvent.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage), ct: ct);

	    await _interactions.CreateFollowupMessageAsync
		    (
		     gatewayEvent.ApplicationID,
		     gatewayEvent.Token,
		     HelpMessage,
		     flags: MessageFlags.Ephemeral
		    );
	    
	    return Result.FromSuccess();
    }
}
