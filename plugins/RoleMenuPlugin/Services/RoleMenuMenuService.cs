using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;
using RoleMenuPlugin.Database;
using MessageFlags = Remora.Discord.API.Abstractions.Objects.MessageFlags;

namespace RoleMenuPlugin;

public class RoleMenuMenuService
{
    private const string Version         = "V3";
    private const string CreationMessage = $"Silk! Role Menu Creator {Version}";

    private readonly IDiscordRestChannelAPI       _channels;
    private readonly IDiscordRestInteractionAPI   _interactions;
    private readonly ILogger<RoleMenuMenuService> _logger;

    public RoleMenuMenuService
    (
        IDiscordRestChannelAPI channels,
        IDiscordRestInteractionAPI interactions, 
        ILogger<RoleMenuMenuService> logger
    )
    {
        _channels = channels;
        _interactions = interactions;
        _logger       = logger;
    }
    
    #region Misc / Multi-Category

    private const string CancelledMessage = "Cancelled!";

    private readonly ButtonComponent _cancelButton = new ButtonComponent(ButtonComponentStyle.Danger, "Cancel", CustomID: "rm-cancel");
    
    #endregion

    #region Main Menu Buttons & Text
    
    private const string MainMenuText        = $"{CreationMessage} | Use the buttons below to create a role menu!";
    private const string MainMenuWaitingText = $"{CreationMessage} | Waiting for input...";
    
    private readonly ButtonComponent _addMenuInteractiveButton  = new(ButtonComponentStyle.Primary,     "Add (Interactive)", CustomID: "rm-add-interactive");
    private readonly ButtonComponent _addMenuSimpleButton       = new (ButtonComponentStyle.Secondary,  "Add (Simple)",      CustomID: "rm-add-role-only");
    private readonly ButtonComponent _addMenuEditButton         = new (ButtonComponentStyle.Secondary,  "Edit Option",       CustomID: "rm-edit-options", IsDisabled: true);
    
    private readonly ButtonComponent _addMenuHelpButton         = new(ButtonComponentStyle.Secondary,   "Help",      CustomID: "rm-help");
    private readonly ButtonComponent _addMenuFinishButton       = new(ButtonComponentStyle.Success,     "Finish",    CustomID: "rm-finish", IsDisabled: true);
    private readonly ButtonComponent _addMenuCancelButton       = new(ButtonComponentStyle.Danger,      "Cancel",    CustomID: "rm-cancel");
    
    #endregion

    #region  Edit Menu Buttons & Text
        
    private const string EditMenuText = $"Silk! Role Menu Creator {Version} | Use the buttons below to edit the role menu!";
    
    private readonly ButtonComponent _editMenuRemoveOptionButton    = new(ButtonComponentStyle.Danger,      "Remove this option",   CustomID: "rm-edit-remove-option");
    private readonly ButtonComponent _editMenuChangeRoleButton      = new(ButtonComponentStyle.Primary,     "Change Role",          CustomID: "rm-edit-change-role");
    
    private readonly ButtonComponent _editMenuAddDescriptionButton  = new(ButtonComponentStyle.Success,     "Add Description",      CustomID: "rm-edit-add-description");
    private readonly ButtonComponent _editMenuAddEmojiButton        = new(ButtonComponentStyle.Success,     "Add Emoji",            CustomID: "rm-edit-add-emoji");
    
    private readonly ButtonComponent _editMenuEditDescriptionButton = new(ButtonComponentStyle.Secondary,   "Edit Description",     CustomID: "rm-edit-edit-description");
    private readonly ButtonComponent _editMenuEditEmojiButton       = new(ButtonComponentStyle.Secondary,   "Edit Emoji",           CustomID: "rm-edit-edit-emoji");
    
    private readonly ButtonComponent _editMenuRemoveButton          = new(ButtonComponentStyle.Danger,      "Remove",               CustomID: "rm-edit-remove-selection");
    private readonly ButtonComponent _editMenuCancelButton          = new(ButtonComponentStyle.Danger,      "Cancel",               CustomID: "rm-edit-cancel");

    #endregion

    #region Input Messages

    private const string AddRoleInputMessage        = "Which role?                            \n(Refer to the help menu for more information!)";
    private const string AddDescriptionInputMessage = "Description?   (Type `skip` to skip!)? \n(Refer to the help menu for more information!)";
    private const string AddEmojiInputMessage       = "Want an emoji? (Type `skip` to skip!)? \n(Refer to the help menu for more information!)";
    
    #endregion

    #region Input Error Messages

    private const string RoleNotFoundErrorMessage                 = "Sorry, but I can't seem to find that role. Are you sure you typed it correctly?";
    private const string RoleAlreadyExistsErrorMessage            = "Sorry, but that role has already been added! You need to choose a different role!";
    private const string RoleUnassignableHierarchyErrorMessage    = "Sorry, but you need to move this role below my own before I can assign it to others!";
    private const string RoleUnassignableIntegrationErrorMessage  = "Sorry, but I can't assign this role; it belongs to a bot!";
    private const string RoleUnassignableEveryoneRoleErrorMessage = "Sorry, but I can't assign this role; it belongs to everyone!";
    private const string DescriptionTooLongErrorMessage           = "Sorry, but your description is too long! There's a maxiumum of 100 characters.";
    private const string EmojiNotFoundErrorMessage                = "Sorry, but I can't seem to find that emoji. Are you sure you typed it correctly?";
    
    #endregion
    
    #region Transition Method (Interaction Based)
    
    /// <summary>
    /// Transitions the menu to a state of displaying the main options,
    /// disabling appropriate buttons, and setting the content of the message.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="currentOptions">The current options being set up, to determine which buttons should be disabled.</param>
    public async Task<IResult> TransitionToMainMenuAsync(IInteraction interaction, IReadOnlyList<RoleMenuOptionModel> currentOptions)
    {
        var addFullButtonWithState = _addMenuInteractiveButton with { IsDisabled = currentOptions.Count >= 25 };
        var addButtonWithState     = _addMenuSimpleButton     with { IsDisabled = currentOptions.Count >= 25 };
        var editButtonWithState    = _addMenuEditButton    with { IsDisabled = currentOptions.Count <=  0 };
        var finishButtonWithState  = _addMenuFinishButton  with { IsDisabled = currentOptions.Count <=  0 };

        var result = await _interactions.CreateInteractionResponseAsync
            (
             interaction.ID,
             interaction.Token,
             new InteractionResponse
                 (
                  InteractionCallbackType.UpdateMessage,
                  new InteractionCallbackData
                      (
                          Content: MainMenuText,
                          Components: new IMessageComponent[]
                          {
                              new ActionRowComponent(new IMessageComponent[]
                              {
                                  addFullButtonWithState,
                                  addButtonWithState,
                                  editButtonWithState,
                              }),
                              new ActionRowComponent(new IMessageComponent[]
                              {
                                  _addMenuHelpButton,
                                  finishButtonWithState,
                                  _addMenuCancelButton,
                              }),
                          }
                      )
                 )
            );

        if (result.IsSuccess)
        {
            _logger.LogDebug("Successfully transitioned to main menu.");
            return Result.FromSuccess();
        }
        else
        {
            var userID  = interaction.Member.IsDefined(out var member) ? member.User.Value.ID.ToString() : "Unknown";
            var guildID = interaction.GuildID.IsDefined(out var guild) ? guild.ToString() : "Unknown";
            
            _logger.LogWarning("Encountered an error while attempting to transition to main menu. Guild: {Guild}, User: {User}, Error: {Error}",
                               guildID, userID,
                               ((RestResultError<RestError>)result.Error).Message);

            return result;
        }
    }

    /// <summary>
    /// Transitions the menu of displaying the main options,
    /// but all of the buttons will be disabled, including the cancel button.
    ///
    /// The user will see that input is being awaited.
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public async Task<IResult> TransitionToDisabledMainMenuAsync(IInteraction interaction)
    {
        var result = await _interactions.CreateInteractionResponseAsync
            (
             interaction.ID,
             interaction.Token,
             new InteractionResponse
                 (
                  InteractionCallbackType.UpdateMessage,
                  new InteractionCallbackData
                      (
                        Content: MainMenuWaitingText,
                        Components: new IMessageComponent[]
                        {
                           new ActionRowComponent
                           (
                            new IMessageComponent[]
                               {
                                   _addMenuInteractiveButton    with { IsDisabled = true },
                                   _addMenuSimpleButton         with { IsDisabled = true },
                                   _addMenuEditButton           with { IsDisabled = true },
                               }
                           ),
                           new ActionRowComponent
                           (
                            new IMessageComponent[]
                               {
                                   _addMenuHelpButton                                     ,
                                   _addMenuFinishButton         with { IsDisabled = true },
                                   _addMenuCancelButton         with { IsDisabled = true },
                               }
                           )
                        }
                      )
                 )
            );

        if (result.IsSuccess)
        {
            _logger.LogDebug("Successfully transitioned to disabled main menu.");
            return Result.FromSuccess();
        }
        else
        {
            var userID = interaction.Member.IsDefined(out var member) ? member.User.Value.ID.ToString() : "Unknown";
            var guildID = interaction.GuildID.IsDefined(out var guild) ? guild.ToString() : "Unknown";

            _logger.LogWarning("Encountered an error while attempting to transition to disabled main menu. Guild: {Guild}, User: {User}, Error: {Error}",
                               guildID, userID,
                               ((RestResultError<RestError>)result.Error).Message);

            return result;
        }
    }

    /// <summary>
    /// Transitions a message into a state of being 'cancelled', displaying a
    /// cancelled message, and removing any components that existed on the message.
    /// </summary>
    /// <param name="interaction">The interaction to update the message with.</param>
    public async Task<IResult> TransitionToCancelledMessageAsync(IInteraction interaction)
    {
        var result = await _interactions
           .CreateInteractionResponseAsync
                (
                 interaction.ID,
                 interaction.Token,
                 new InteractionResponse
                     (
                      InteractionCallbackType.UpdateMessage,
                      new InteractionCallbackData
                          (
                           Content: CancelledMessage,
                           Components: Array.Empty<IMessageComponent>()
                          )
                     )
                );

        if (result.IsSuccess)
        {
            _logger.LogDebug("Successfully transitioned to cancelled message.");
            
            return Result.FromSuccess();
        }
        else
        {
            var userID  = interaction.Member.IsDefined(out var member) ? member.User.Value.ID.ToString() : "Unknown";
            var guildID = interaction.GuildID.IsDefined(out var guild) ? guild.ToString() : "Unknown";

            _logger.LogWarning("Encountered an error while attempting to transition to disabled main menu. Guild: {Guild}, User: {User}, Error: {Error}",
                               guildID, userID,
                               ((RestResultError<RestError>)result.Error).Message);

            return result;
        }
    }
    
    #endregion

    #region Display Methods (Non Idempotent)

    public async Task<IResult> DisplayMainMenuAsync(Snowflake channelID)
    {
        return await _channels.CreateMessageAsync
            (
             channelID,
             MainMenuText,
             components: new IMessageComponent[]
             {
                 new ActionRowComponent(new IMessageComponent[]
                 {
                     _addMenuInteractiveButton,
                     _addMenuSimpleButton,
                     _addMenuEditButton,
                 }),
                 new ActionRowComponent(new IMessageComponent[]
                 {
                     _addMenuHelpButton,
                     _addMenuFinishButton,
                     _addMenuCancelButton
                 })
             });
    }
    
    public Task<IResult> DisplayRoleInputAsync(IInteraction interaction)
        => DisplayCancelableInputAsync(interaction, AddRoleInputMessage);

    public Task<IResult> DisplayEmojiInputAsync(IInteraction interaction)
        => DisplayCancelableInputAsync(interaction, AddEmojiInputMessage);
    
    public Task<IResult> DisplayDescriptionInputAsync(IInteraction interaction)
        => DisplayCancelableInputAsync(interaction, AddDescriptionInputMessage);
    
    
    /// <summary>
    /// Displays a cancellable input message to the user.
    /// </summary>
    /// <param name="interaction">The interaction to use to create a followup message with.</param>
    /// <param name="message">The message to display to the user.</param>
    private async Task<IResult> DisplayCancelableInputAsync(IInteraction interaction, string message)
    {
        return await _interactions.CreateFollowupMessageAsync
            (
             interaction.ApplicationID,
             interaction.Token,
             message,
             flags: MessageFlags.Ephemeral,
             components: new IMessageComponent[]
             {
                 new ActionRowComponent(new IMessageComponent[]
                 {
                     _cancelButton
                 })
             });
    }

    /// <summary>
    /// Displays an ephemeral message informing the user the role they're trying to use couldn't be found.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayRoleNotFoundErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, RoleNotFoundErrorMessage);

    /// <summary>
    /// Displays an ephemeral message informing the user the role exists, but has already been added.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayRoleAddedErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, RoleAlreadyExistsErrorMessage);

    /// <summary>
    /// Displays an ephemeral message informing the user the role exists, but is above the bot's.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayRoleHierarchyErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, RoleUnassignableHierarchyErrorMessage);

    /// <summary>
    /// Displays an ephemeral message informing the user the role exists, but belongs to a bot.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayRoleIntegrationErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, RoleUnassignableIntegrationErrorMessage);

    /// <summary>
    /// Displays an ephemeral message informing the user the role exists, but is unassignable because the @everyone role is not assignable.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayRoleEveryoneErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, RoleUnassignableEveryoneRoleErrorMessage);
    
    /// <summary>
    /// Displays an ephemeral message informing the user the that the description exceeds the maximum length.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayDescriptionLengthErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, DescriptionTooLongErrorMessage);
    
    /// <summary>
    /// Displays an ephemeral message informing the user the emoji could not be determined.
    /// </summary>
    /// <param name="interaction">The interaction to use to post the new message</param>
    public Task<IResult> DisplayEmojiNotFoundErrorAsync(IInteraction interaction)
        => DisplayInputErrorAsync(interaction, EmojiNotFoundErrorMessage);

    
    private async Task<IResult> DisplayInputErrorAsync(IInteraction interaction, string error)
    {
        await _interactions.CreateInteractionResponseAsync(interaction.ID, interaction.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));

        return await _interactions.CreateFollowupMessageAsync(interaction.ApplicationID, interaction.Token, error, flags: MessageFlags.Ephemeral);
    }

    #endregion
}