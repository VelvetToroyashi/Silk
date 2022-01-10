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


	    var sb = new StringBuilder()
		   .AppendLine("**How to use this thing**")
		   .AppendLine("There are a bombardment of options, and you may be curious as to what they do.")
		   .AppendLine()
		   .AppendLine("From left to right, I will explain what all the buttons are for.")
		   .AppendLine("`Add option(full)`:")
		   .Append("\u200b\t")
		   .AppendLine("This option is the interactive way of adding roles, but can be a tad slow.")
		   .Append("\u200b\t")
		   .AppendLine("Using this button will prompt you for the role, an emoji to go with it, and the description.")
		   .Append("\u200b\t")
		   .AppendLine("For the role, it must not be `@everyone`, nor above either of our top roles. I can't assign those!")
		   .Append("\u200b\t")
		   .AppendLine("You can either mention the role directly, or type its name.")
		   .Append("\u200b\t")
		   .AppendLine("For the emoji, you can use any emoji, but they must be typed out properly.")
		   .Append("\u200b\t")
		   .AppendLine("(e.g. <a:catgiggle:853806288190439436> or 👋 and not catgiggle or \\:wave\\:)")
		   .Append("\u200b\t")
		   .AppendLine("Descriptions are also easy. They can be whatever you want, but they will limited to 100 characters.")
		   .AppendLine()
		   .AppendLine("`Add option(role only)`:")
		   .Append("\u200b\t")
		   .AppendLine("This is a faster, but more restricted way of adding roles.")
		   .Append("\u200b\t")
		   .AppendLine("You can only add the role, but you can add them in batches.")
		   .Append("\u200b\t")
		   .AppendLine("When using this option, you must mention the role directly (e.g. `@role`).")
		   .Append("\u200b\t")
		   .AppendLine("If you'd like to retro-actively add an emoji or description, you can use the edit button.")
		   .Append("\u200b\t")
		   .AppendLine("You can't add the `@everyone` role, nor above either of our top roles.")
		   .AppendLine()
		   .AppendLine("`Edit option`:")
		   .Append("\u200b\t")
		   .AppendLine("This button allows you to edit options for the current role menu being setup.")
		   .AppendLine("After selecting the option you want to edit, you can perform several actions with the provided buttons.")
		   .AppendLine()
		   .AppendLine("`Finish`:")
		   .Append("\u200b\t")
		   .AppendLine("This is the final button. It will send the role menu to the channel you specified.")
		   .AppendLine("First, you must confirm that you want to finish the creation of this role menu.")
		   .AppendLine("You will be presented with a dropdown of all the options you've added.")
		   .AppendLine("Clicking confirm will send the role menu to the channel you specified.")
		   .AppendLine()
		   .AppendLine("`Quit`:")
		   .Append("\u200b\t")
		   .AppendLine("This will cancel the role menu and delete the message you started it with.")
		   .AppendLine()
		   .AppendLine("**Note**:")
		   .Append("\u200b\t")
		   .AppendLine("If you're not sure what to do, try the `Add option(full)` button first.")
		   .Append("\u200b\t")
		   .AppendLine("Also, this is considered beta software, so please report any bugs you find!.");

	    await _interactions.CreateInteractionResponseAsync(gatewayEvent.ID, gatewayEvent.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage), ct: ct);

	    await _interactions.CreateFollowupMessageAsync
		    (
		     gatewayEvent.ApplicationID,
		     gatewayEvent.Token,
		     sb.ToString(),
		     flags: MessageFlags.Ephemeral
		    );
	    
	    return Result.FromSuccess();
    }
}
