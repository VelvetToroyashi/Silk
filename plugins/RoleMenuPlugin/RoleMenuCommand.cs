using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using FuzzySharp;
using FuzzySharp.Extractor;
using MediatR;
using Remora.Results;
using RoleMenuPlugin.Database;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin
{
	/// <summary>
	///     The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
    [Aliases("role-menu", "rm")]
    [Description("Role menu related commands.")]
    //[RequirePermissions(Permissions.ManageRoles)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public sealed class RoleMenuCommand : BaseCommandModule
    {
        private const int MessageReadDelayMs = 2200;

        private static readonly DiscordButtonComponent _quitButton        = new(ButtonStyle.Danger, "rm-quit", "Quit");
        private readonly        DiscordButtonComponent _addFullButton     = new(ButtonStyle.Primary, "rm-add-full", "Add option (full)");
        private readonly        DiscordButtonComponent _addRoleOnlyButton = new(ButtonStyle.Secondary, "rm-add", "Add option (role only)");
        private readonly        DiscordClient          _client;

        private readonly DiscordButtonComponent _editButton   = new(ButtonStyle.Primary, "rm-edit", "Edit the current options", true);
        private readonly DiscordButtonComponent _finishButton = new(ButtonStyle.Success, "rm-finish", "Finish", true);

        private readonly DiscordButtonComponent _htuButton = new(ButtonStyle.Primary, "rm-htu", "How do I use this thing???");

        private readonly IMediator _mediator;

        private readonly DiscordButtonComponent _v1ExplainationButton = new(ButtonStyle.Secondary, "rm-explain-V1", "Why can't I edit my menu?");

        private readonly string V1CutoffDate = Formatter.Timestamp(new DateTime(2021, 12, 10), TimestampFormat.ShortDate);

        public RoleMenuCommand(IMediator mediator, DiscordClient client)
        {
            _mediator = mediator;
            _client   = client;

            _client.ComponentInteractionCreated += ExplainV1Async;
        }

        private async Task ExplainV1Async(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id == "rm-explain-V1")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);



                await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                                              .WithContent("Question: \"Why can't I edit my role menu?\"\n\n"                          +
                                                                           "Answer: "                                                                  +
                                                                           $"Unfortunately, V1 role menus (anything created prior to {V1CutoffDate}) " +
                                                                           "do not store information about the channel they were created for.\n"       +
                                                                           "If you wish to edit a V1 role menu, it's advised that you recreate it. Our role menu creator has been greatly improved, too!")
                                                              .AsEphemeral(true));
            }
        }

        [Command]
        [Description("Create a new role-menu. \n\n"                                        +
                     "**Disclaimer**:\n\n"                                                 +
                     "V2 of this command is currently considered beta software.\n"         +
                     "It however, is generally considered stable, and has been shipped.\n" +
                     "If you experience any issues when creating a role menu, contact support.")]
        public async Task Create(CommandContext ctx, DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;

            if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
            {
                await ctx.RespondAsync("I don't have permission to send messages in that channel.");
                return;
            }

            DiscordMessage initialMenuMessage = await ctx.RespondAsync("Warming up...");

            await Task.Delay(600);

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            var options = new List<RoleMenuOptionModel>();

            var reset = true;

            while (true)
            {
                if (reset)
                {
                    ResetToMenu(ref initialMenuMessage);
                    reset = false;
                }

                ComponentInteractionCreateEventArgs? selection   = (await interactivity.WaitForButtonAsync(initialMenuMessage, ctx.User, CancellationToken.None)).Result;
                string?                              selectionId = selection.Id;

                Task t = selectionId switch
                {
                    "rm-quit"     => Task.CompletedTask,
                    "rm-finish"   => Task.CompletedTask,
                    "rm-edit"     => Edit(ctx, selection.Interaction, interactivity, options),
                    "rm-add-full" => AddFull(ctx, selection.Interaction, options, interactivity),
                    "rm-add"      => AddRoleOnly(ctx, selection.Interaction, options, interactivity),
                    "rm-htu"      => ShowHelpAsync(selection.Interaction),
                    _             => Task.CompletedTask
                };

                _addFullButton.Disable();
                _addRoleOnlyButton.Disable();
                _editButton.Disable();
                _finishButton.Disable();
                _quitButton.Disable();
                _htuButton.Disable();

                await selection
                     .Interaction
                     .CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                                                                .WithContent(initialMenuMessage.Content)
                                                                                .AddComponents(_addFullButton, _addRoleOnlyButton, _editButton)
                                                                                .AddComponents(_finishButton, _quitButton, _htuButton));

                await t;

                if (selectionId == "rm-quit")
                {
                    await initialMenuMessage.DeleteAsync();

                    try
                    {
                        await ctx.Message.DeleteAsync();
                    }
                    catch
                    {
                        // ignored
                    }

                    return;
                }
                if (selectionId == "rm-finish")
                {
                    if (await ConfirmFinishedAsync(selection.Interaction, interactivity, options))
                    {
                        await selection.Interaction.EditOriginalResponseAsync(new()
                        {
                            Content = "Thank you for choosing Silk! Your role menu has been deployed to the specified channel."
                        });

                        try
                        {
                            await ctx.Message.DeleteAsync();
                        }
                        catch
                        {
                            //Ignored
                        }

                        break;
                    }
                }

                _editButton.Enable();
                _quitButton.Enable();
                _htuButton.Enable();

                if (options.Count >= 1)
                {
                    _finishButton.Enable();
                    _editButton.Enable();
                }
                else
                {
                    _finishButton.Disable();
                    _editButton.Disable();
                }

                if (options.Count >= 25)
                {
                    _addFullButton.Disable();
                    _addRoleOnlyButton.Disable();
                }
                else
                {
                    _addFullButton.Enable();
                    _addRoleOnlyButton.Enable();
                }

                try
                {
                    await selection.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                                                         .WithContent("Silk! Role Menu Creator v2.0")
                                                                         .AddComponents(_addFullButton, _addRoleOnlyButton, _editButton)
                                                                         .AddComponents(_finishButton, _quitButton, _htuButton));

                    reset = false;
                }
                catch
                {
                    // Interaction timed out
                    reset = true;
                }
            }

            // if we're here, we're done
            StringBuilder outputMessageBuilder = new StringBuilder()
                                                .AppendLine(Formatter.Bold("Role Menu:"))
                                                .AppendLine("Click the button below to view the role menu.")
                                                .AppendLine("Available roles are:")
                                                .AppendLine()
                                                .AppendLine(string.Join("\n", options.Select(x => $"<@&{x.RoleId}>")));

            DiscordMessage? rmMessage = await channel.SendMessageAsync(m => m
                                                                           .WithContent(outputMessageBuilder.ToString())
                                                                           .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, RoleMenuRoleService.RoleMenuPrefix, "Get Roles")));

            var roleMenu = new RoleMenuModel
            {
                MessageId = rmMessage.Id,
                ChannelId = channel.Id,
                GuildId   = ctx.Guild.Id,
                Options   = options
            };

            await _mediator.Send(new CreateRoleMenu.Request(roleMenu));
        }

        private async Task<bool> ConfirmFinishedAsync(DiscordInteraction interaction, InteractivityExtension interactivity, List<RoleMenuOptionModel> options)
        {
            IEnumerable<DiscordSelectComponentOption> select = options
               .Select(x => new DiscordSelectComponentOption(x.RoleName, x.RoleId.ToString(), x.Description, false, x.EmojiName is null ? null :
                                                             ulong.TryParse(x.EmojiName, out ulong id)                                  ? new(id) : new(x.EmojiName)));

            DiscordMessage? message = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                                                                  .WithContent("Please confirm you want to finish the creation of this role menu by verifying the options below.\n" +
                                                                                               "This is the same dropdown users will see when they select their roles!")
                                                                                  .AddComponents(new DiscordSelectComponent(interaction.Id.ToString(), "Select your roles!", select, false, 0, options.Count))
                                                                                  .AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "rm-confirm", "Confirm"),
                                                                                                 new DiscordButtonComponent(ButtonStyle.Danger, "rm-cancel", "Cancel"))
                                                                                  .AsEphemeral(true));

            InteractivityResult<ComponentInteractionCreateEventArgs> res = await interactivity.WaitForButtonAsync(message, interaction.User, TimeSpan.FromMinutes(14));

            if (!res.TimedOut)
                await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (res.TimedOut || res.Result.Id == "rm-cancel")
                return false;

            return true;
        }

        private async Task ShowHelpAsync(DiscordInteraction interaction)
        {
            //write the help text using a string builder
            var sb = new StringBuilder();
            sb
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

            await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent(sb.ToString()).AsEphemeral(true));
        }

        private static async Task AddFull(CommandContext ctx, DiscordInteraction interaction, List<RoleMenuOptionModel> options, InteractivityExtension interactivity)
        {
            DiscordRole?  role;
            DiscordEmoji? emoji;
            string?       description;

            DiscordMessage tipMessage = await interaction.CreateFollowupMessageAsync(new() { Content = "\u200b", IsEphemeral = true });

            role = await GetRoleAsync(ctx, interaction, interactivity, tipMessage, options);

            if (role is null)
                return;

            InputResult<DiscordEmoji?> emojiRes = await GetEmojiAsync(ctx, interaction, interactivity, tipMessage);

            if (emojiRes.Cancelled)
                return;

            emoji = emojiRes.Value;

            description = await GetDescriptionAsync(interaction, interactivity, tipMessage);

            bool confirm = await GetConfirmationAsync();

            if (!confirm)
                return;

            options.Add(new()
            {
                RoleId      = role.Id,
                RoleName    = role.Name,
                EmojiName   = emoji?.Name,
                Description = description,
                GuildId     = ctx.Guild.Id,
            });

            async Task<bool> GetConfirmationAsync()
            {
                DiscordMessage? confirmMessage = await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                                                                                                          .WithContent("Are you sure you want to add this role to the menu?\n" +
                                                                                                                       $"Role: {role.Name}\n"                                  +
                                                                                                                       $"Emoji: {emoji}\n"                                     +
                                                                                                                       $"Description: {description ?? "None"}")
                                                                                                          .AddComponents(
                                                                                                                         new DiscordButtonComponent(ButtonStyle.Success, "y", "Yes"),
                                                                                                                         new DiscordButtonComponent(ButtonStyle.Danger, "n", "No (Cancel)")
                                                                                                                        ));

                InteractivityResult<ComponentInteractionCreateEventArgs> res = await interactivity.WaitForButtonAsync(confirmMessage, TimeSpan.FromMinutes(14));

                bool ret = res.Result?.Id switch
                {
                    "y" => true,
                    "n" => false,
                    _   => false
                };

                if (!res.TimedOut)
                {
                    await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                                                     new DiscordInteractionResponseBuilder()
                                                                        .WithContent(ret ? "Added role to menu." : "Cancelled."));
                }

                return ret;
            }
        }

        private static async Task AddRoleOnly(CommandContext ctx, DiscordInteraction interaction, List<RoleMenuOptionModel> options, InteractivityExtension interactivity)
        {
            DiscordMessage? message = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                                                                  .WithContent("Please mention the roles you'd like to add to the menu.")
                                                                                  .AsEphemeral(true));

            var erroredResponsesBuilder = new StringBuilder();

            InteractivityResult<DiscordMessage> res = await interactivity.WaitForMessageAsync(m => m.MentionedRoles.Any(), TimeSpan.FromMinutes(14));

            IReadOnlyList<DiscordRole>? roles          = res.Result.MentionedRoles;
            int                         availableSlots = 25 - roles.Count;
            var                         added          = 0;

            foreach (DiscordRole role in roles)
            {
                if (added >= availableSlots)
                    break;

                if (options.Any(o => o.RoleId == role.Id))
                {
                    erroredResponsesBuilder.AppendLine($"{role.Mention} is already in the menu.");
                    continue;
                }

                if (role == ctx.Guild.EveryoneRole)
                {
                    erroredResponsesBuilder.AppendLine("You can't add the everyone role to the menu.");
                    continue;
                }

                if (role.IsManaged)
                {
                    erroredResponsesBuilder.AppendLine($"{role.Mention} is managed by a bot/integration, and I can't add it, sorry!");
                    continue;
                }

                if (role.Position >= ctx.Guild.CurrentMember.Roles.Max(r => r.Position))
                {
                    erroredResponsesBuilder.AppendLine($"{role.Mention} is above my highest role, and I can't add it, sorry!");
                    continue;
                }

                if (role.Position >= (interaction.User as DiscordMember)!.Roles.Max(r => r.Position))
                {
                    erroredResponsesBuilder.AppendLine($"{role.Mention} is above your highest role, and I can't add it, sorry!");
                    continue;
                }

                added++;

                options.Add(new()
                {
                    RoleId   = role.Id,
                    RoleName = role.Name,
                    GuildId  = ctx.Guild.Id,
                });
            }

            await interaction.EditFollowupMessageAsync(message.Id, new DiscordWebhookBuilder()
                                                          .WithContent($"Added {Formatter.Bold(added.ToString())} roles to the menu." +
                                                                       (erroredResponsesBuilder.Length is 0 ? "" : $"\n\nThere were some issues with some of the roles, and they will not be added.\n{erroredResponsesBuilder}")));
        }

        private static async Task Edit(CommandContext ctx, DiscordInteraction interaction, InteractivityExtension interactivity, List<RoleMenuOptionModel> options)
        {
            IEnumerable<DiscordSelectComponentOption> sopts = options.Select((x, i) =>
                                                                                 new DiscordSelectComponentOption(x.RoleName, i.ToString(), x.Description));

            DiscordMessage? selectionMessage = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral(true)
                                                                                                                               .WithContent("\u200b")
                                                                                                                               .AddComponents(new DiscordSelectComponent("rm-edit-current", "Select the option you want to edit", sopts))
                                                                                                                               .AddComponents(_quitButton));

            Wait:
            Task<InteractivityResult<ComponentInteractionCreateEventArgs>>? t1 = interactivity.WaitForButtonAsync(selectionMessage, TimeSpan.FromMinutes(14));
            Task<InteractivityResult<ComponentInteractionCreateEventArgs>>? t2 = interactivity.WaitForSelectAsync(selectionMessage, "rm-edit-current", TimeSpan.FromMinutes(14));

            InteractivityResult<ComponentInteractionCreateEventArgs> res = (await Task.WhenAny(t1, t2)).Result;

            if (!res.TimedOut && res.Result.Id != "rm-quit")
            {
                await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            }
            else
            {
                await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));
                return;
            }

            int                 index  = int.Parse(res.Result.Values[0]);
            RoleMenuOptionModel option = options[index]; // Default min value is 1, so there's always an index.

            var changeRoleButton        = new DiscordButtonComponent(ButtonStyle.Primary, "rm-change-role", "Change Role");
            var changeEmojiButton       = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-change-emoji", "Change Emoji");
            var changeDescriptionButton = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-change-description", "Change Description");
            var deleteButton            = new DiscordButtonComponent(ButtonStyle.Danger, "rm-delete", "Delete");

            var addEmojiButton       = new DiscordButtonComponent(ButtonStyle.Success, "rm-add-emoji", "Add Emoji");
            var addDescriptionButton = new DiscordButtonComponent(ButtonStyle.Success, "rm-add-description", "Add Description");

            var quitButton = new DiscordButtonComponent(ButtonStyle.Danger, "rm-quit", "Exit");


            while (true)
            {
                //TODO: Add buttons to make an option mutually exclusive.

                selectionMessage = await res.Result.Interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder()
                                                                                                             .WithContent($"Editing option {index + 1}")
                                                                                                             .AddEmbed(new DiscordEmbedBuilder()
                                                                                                                      .WithColor(DiscordColor.Wheat)
                                                                                                                      .WithTitle("Current menu option:")
                                                                                                                      .AddField("Role", option.RoleName, true)
                                                                                                                      .AddField("Emoji", option.EmojiName is null                 ? "Not set." :
                                                                                                                                ulong.TryParse(option.EmojiName, out ulong emoji) ? $"<a:{emoji}>" : option.EmojiName, true)
                                                                                                                      .AddField("Description", option.Description ?? "None"))
                                                                                                             .AddComponents(changeRoleButton, option.EmojiName is null ? addEmojiButton : changeEmojiButton, option.Description is null ? addDescriptionButton : changeDescriptionButton, deleteButton, quitButton));
                res = await interactivity.WaitForButtonAsync(selectionMessage, TimeSpan.FromMinutes(14));

                if (res.TimedOut)
                {
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));
                    return;
                }

                Task t = res.Result.Id switch
                {
                    "rm-change-role"        => ChangeRoleAsync(),
                    "rm-change-emoji"       => ChangeEmojiAsync(),
                    "rm-change-description" => ChangeDescriptionAsync(),
                    "rm-delete"             => DeleteAsync(),
                    "rm-add-emoji"          => AddEmojiAsync(),
                    "rm-add-description"    => AddDescriptionAsync(),
                    "rm-quit"               => Task.CompletedTask,
                    _                       => throw new ArgumentException("Invalid button id.")
                };

                await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                await t;

                if (res.Result.Id == "rm-delete")
                {
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Done. That option is no longer a part of the menu. \nThis action cannot be undone."));
                    await Task.Delay(MessageReadDelayMs);

                    selectionMessage = await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder()
                                                                                                      .WithContent("\u200b")
                                                                                                      .AddComponents(new DiscordSelectComponent("rm-edit-current", "Select the option you want to edit", sopts))
                                                                                                      .AddComponents(_quitButton));

                    goto Wait;
                }

                if (res.TimedOut || res.Result.Id == "rm-quit")
                {
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));
                    return;
                }
            }

            async Task ChangeRoleAsync()
            {
                DiscordRole? ret = await GetRoleAsync(ctx, interaction, interactivity, selectionMessage, options);

                if (ret is not null)
                {
                    option.RoleId   = ret.Id;
                    option.RoleName = ret.Name;

                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Role changed successfully!"));
                }
            }

            async Task ChangeEmojiAsync()
            {
                InputResult<DiscordEmoji?> ret = await GetEmojiAsync(ctx, interaction, interactivity, selectionMessage);

                if (ret.Value is not null)
                {
                    option.EmojiName = ret.Value;

                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Emoji changed successfully!"));
                }
            }

            async Task ChangeDescriptionAsync()
            {
                string? ret = await GetDescriptionAsync(interaction, interactivity, selectionMessage);

                if (ret is not null)
                {
                    option.Description = ret;
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Description changed successfully!"));
                }
            }

            Task DeleteAsync()
            {
                options.Remove(option);
                //options.RemoveAt();
                return Task.CompletedTask;
            }

            async Task AddEmojiAsync()
            {
                InputResult<DiscordEmoji?> ret = await GetEmojiAsync(ctx, interaction, interactivity, selectionMessage);


                if (!ret.Cancelled)
                {
                    option.EmojiName = ret.Value?.ToString();
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Emoji added successfully!"));
                }
            }

            async Task AddDescriptionAsync()
            {
                string? ret = await GetDescriptionAsync(interaction, interactivity, selectionMessage);

                if (ret is not null)
                {
                    option.Description = ret;
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Description added successfully!"));
                }
            }

        }

        private static async Task<string?> GetDescriptionAsync(DiscordInteraction interaction, InteractivityExtension interactivity, DiscordMessage tipMessage)
        {
            string? description = null;
            while (true)
            {
                await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Enter a description for this role.\n"                +
                                                                                                                  "Descriptions will be truncated at 100 characters.\n" +
                                                                                                                  "Type `cancel` to cancel adding this role. Type `skip` to skip adding a description."));

                InputResult<DiscordMessage?> input = await GetInputAsync(interactivity, interaction, tipMessage.Id);

                if (input.Cancelled)
                    return description;

                description = input.Value?.Content?.Length > 100 ? input.Value.Content[..100] : input.Value?.Content;

                if (input.Value is not null)
                    await input.Value.DeleteAsync();

                return description;
            }
        }

        private static async Task<InputResult<DiscordEmoji?>> GetEmojiAsync(CommandContext ctx, DiscordInteraction interaction, InteractivityExtension interactivity, DiscordMessage interactionMessage)
        {
            DiscordEmoji? emoji = null;
            while (true)
            {
                await interaction.EditFollowupMessageAsync(interactionMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("Enter the emoji you want to use to represent this role.\n"                                                                        +
                                                                           "If you don't see the emoji in the list, you may need to type it in the exact format it appears in the server (e.g. `:emoji:`).\n" +
                                                                           "Type `cancel` to cancel adding this role. Type `skip` to skip adding an emoji."));

                InputResult<DiscordMessage?> input = await GetInputAsync(interactivity, interaction, interactionMessage.Id);

                if (input.Cancelled)
                    return new(true, null);

                if (input.Value is null)
                    return new(false, null);

                var converter = (IArgumentConverter<DiscordEmoji>)new DiscordEmojiConverter();

                Optional<DiscordEmoji> result = await converter.ConvertAsync(input.Value.Content, ctx);

                if (!result.HasValue)
                {
                    await interaction.EditFollowupMessageAsync(interactionMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that emoji. Try again."));
                    await Task.Delay(MessageReadDelayMs);
                    continue;
                }
                await input.Value.DeleteAsync();

                emoji = result.Value;

                return new(false, emoji);
            }
        }

        private static async Task<DiscordRole?> GetRoleAsync(CommandContext ctx, DiscordInteraction interaction, InteractivityExtension interactivity, DiscordMessage selectionMessage, List<RoleMenuOptionModel> options)
        {
            DiscordRole? role = null;

            while (true)
            {
                await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("Enter the name of the role you want to use for this option.\n"                                                                 +
                                                                           "If you don't see the role in the list, you may need to type it in the exact format it appears in the server (e.g. `@Role`).\n" +
                                                                           "Type `cancel` to cancel adding roles."));

                InputResult<DiscordMessage?> input = await GetInputAsync(interactivity, interaction, selectionMessage.Id);

                if (input.Cancelled)
                    return role;

                if (input.Value!.MentionedRoles.Count is not 0)
                {
                    DiscordRole? r = role = input.Value.MentionedRoles[0];

                    // Ensure the role is not above the user's highest role
                    if (!await EnsureNonDuplicatedRoleAsync() || !await ValidateRoleHeirarchyAsync(ctx, interaction, r, selectionMessage))
                        continue;

                    return r;
                }
                // Accurate route: Use DiscordRoleConverter | This is the most accurate way to get the role, but doesn't support names
                // Less accurate route: Use FuzzySharp to fuzzy match the role name, but use a high drop-off threshold

                //We need to check role names via RoleConverter casted to IArgumentConverter<DiscordRole>
                var roleConverter = (IArgumentConverter<DiscordRole>)new DiscordRoleConverter();

                //Try to convert the input to a role
                Optional<DiscordRole> result = await roleConverter.ConvertAsync(input.Value.Content, ctx);

                if (result.HasValue)
                {
                    role = result.Value;
                }
                else
                {
                    ExtractedResult<(string, ulong)>? fuzzyRes = Process.ExtractSorted((input.Value.Content, default(ulong)),
                                                                                       ctx.Guild.Roles.Select(r => (r.Value.Name, r.Key)),
                                                                                       r => r.Item1, cutoff: 80)
                                                                        .FirstOrDefault();

                    if (fuzzyRes?.Value is null)
                    {
                        await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that role. Try again."));
                        await Task.Delay(MessageReadDelayMs);
                        continue;
                    }

                    role = ctx.Guild.Roles[fuzzyRes.Value.Item2];
                }

                if (!await EnsureNonDuplicatedRoleAsync() || !await ValidateRoleHeirarchyAsync(ctx, interaction, role, selectionMessage))
                    continue;

                await input.Value.DeleteAsync();
                return role;
            }

            async Task<bool> EnsureNonDuplicatedRoleAsync()
            {
                if (options.Any(r => r.RoleId == role.Id))
                {
                    await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("You can't have the same role twice. Try again."));
                    await Task.Delay(MessageReadDelayMs);
                    return false;
                }

                return true;
            }
        }

        private static async Task<bool> ValidateRoleHeirarchyAsync(CommandContext ctx, DiscordInteraction interaction, DiscordRole r, DiscordMessage tipMessage)
        {
            if (r.Position >= ctx.Member.Roles.Max(x => x.Position))
            {
                await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("You can't add roles that are above your highest role."));

                await Task.Delay(MessageReadDelayMs);
                return false;
            }

            if (r.Position >= ctx.Guild.CurrentMember.Roles.Max(x => x.Position))
            {
                await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("I cannot assign that role as it's above my highest role."));

                await Task.Delay(MessageReadDelayMs);
                return false;
            }

            if (r == ctx.Guild.EveryoneRole)
            {
                await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("I cannot assign the everyone role as it's special and cannot be assigned."));

                await Task.Delay(MessageReadDelayMs);
                return false;
            }

            if (r.IsManaged)
            {
                await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                                                              .WithContent("I cannot assign that role as it's managed and cannot be assigned."));

                await Task.Delay(MessageReadDelayMs);
                return false;
            }

            return true;
        }

        private static async Task<InputResult<DiscordMessage?>> GetInputAsync(InteractivityExtension it, DiscordInteraction interaction, ulong message)
        {
            InteractivityResult<DiscordMessage> input = await it.WaitForMessageAsync(m => m.Author == interaction.User, TimeSpan.FromMinutes(14));

            if (input.TimedOut)
                return new(true, null);

            if (input.Result.Content == "cancel")
            {
                await interaction.EditFollowupMessageAsync(message, new DiscordWebhookBuilder().WithContent("Cancelled."));

                await input.Result.DeleteAsync();

                return new(true, null);
            }

            if (input.Result.Content == "skip")
            {
                await interaction.EditFollowupMessageAsync(message, new DiscordWebhookBuilder().WithContent("Skipped."));

                await input.Result.DeleteAsync();

                return new(false, null);
            }

            return new(false, input.Result);
        }


        [Command("edit")]
        [Description("Edits a role menu. To delete a role menu, use `rolemenu delete`")]
        public async Task EditMenuAsync(CommandContext ctx, DiscordChannel? channel = null)
        {
            Result<IEnumerable<RoleMenuModel>> options;

            options = channel is not null
                ? await _mediator.Send(new GetChannelRoleMenusRequest.Request(channel.Id))
                : await _mediator.Send(new GetGuildRoleMenusRequest.Request(ctx.Guild.Id));

            if (!options.IsSuccess)
            {
                await ctx.RespondAsync(options.Error.Message);
                return;
            }

            if (!options.Entity.Any(r => r.ChannelId is not 0))
            {
                await ctx.RespondAsync(m => m.WithContent("There are no role menus to edit.").AddComponents(_v1ExplainationButton));
                return;
            }

            if (!options.Entity.Any())
            {
                await ctx.RespondAsync(m => m.WithContent("There are no role menus to edit.").AddComponents(_v1ExplainationButton));
                return;
            }
            InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

            // Ask the user which menu they want to edit
            IEnumerable<DiscordSelectComponentOption> selectOptions = options.Entity.Where(c => c.ChannelId is not 0)
                                                                             .Select(s =>
                                                                              {
                                                                                  DiscordChannel? chn = ctx.Guild.GetChannel(s.ChannelId);

                                                                                  var option =
                                                                                      new DiscordSelectComponentOption("Role Menu in #" + (chn is null
                                                                                                                           ? "deleted channel"
                                                                                                                           : chn?.Name),
                                                                                                                       s.MessageId.ToString(),
                                                                                                                       $"Message: {s.MessageId} | {options.Entity.Count()} options");

                                                                                  return option;
                                                                              });

            DiscordMessage? msg = await ctx.RespondAsync(m => m.WithContent("There are multiple role menus that can be edited. Which one would you like?")
                                                               .AddComponents(new DiscordSelectComponent("rm-edit-select", "Select an option", selectOptions))
                                                               .AddComponents(_v1ExplainationButton));

            InteractivityResult<ComponentInteractionCreateEventArgs> res = await interactivity.WaitForSelectAsync(msg, ctx.User, "rm-edit-select", TimeSpan.FromMinutes(5));

            if (res.TimedOut)
            {
                await msg.ModifyAsync(m => m.Content = "Timed out.");
                return;
            }

            DiscordInteraction? interaction = res.Result.Interaction;
            RoleMenuModel       selected    = options.Entity.First(x => x.MessageId == ulong.Parse(res.Result.Values[0]));

            await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            var add  = new DiscordButtonComponent(ButtonStyle.Primary, "rm-edit-add", "Add", selected.Options.Count == 25);
            var edit = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-edit-edit", "Edit");
            var quit = new DiscordButtonComponent(ButtonStyle.Danger, "rm-edit-quit", "Quit");


            DiscordMessage? editOrAdd = await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                                                                   .WithContent($"Please select an option. This menu expires {Formatter.Timestamp(DateTimeOffset.UtcNow.AddMinutes(5))}.")
                                                                                   .AddComponents(add, edit, quit));

            while (true)
            {
                res = await editOrAdd.WaitForButtonAsync(TimeSpan.FromMinutes(5));


                if (res.TimedOut)
                {
                    await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("This menu has expired."));
                    break;
                }

                interaction = res.Result.Interaction;
                await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (res.Result.Id == "rm-edit-quit")
                {
                    await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Done. Any changes you've made will be updated."));
                    break;
                }

                if (res.Result.Id == "rm-edit-add")
                {
                    await AddFull(ctx, res.Result.Interaction, selected.Options, interactivity);
                }
                else
                {
                    await Edit(ctx, res.Result.Interaction, interactivity, selected.Options);
                }

                _ = selected.Options.Count == 25 ? add.Disable() : add.Enable();

                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(editOrAdd.Content).AddComponents(add, edit, quit));
            }

            DiscordChannel? roleMenuChannel = ctx.Guild.GetChannel(selected.ChannelId);

            if (roleMenuChannel is null)
                throw new InvalidOperationException("Role menu channel is null");

            DiscordMessage? roleMenuMessage = await roleMenuChannel.GetMessageAsync(selected.MessageId);

            await roleMenuMessage.ModifyAsync(m => m.Content = $"**Role Menu**\nAvailable Roles:\n{string.Join('\n', selected.Options.Select(r => $"<@&{r.RoleId}>"))}");

            await _mediator.Send(new UpdateRoleMenuRequest.Request(selected));
        }

        [Command]
        [Description("Deletes a role menu. Provide a link to the menu to delete.")]
        [Aliases("del")]
        public async Task Delete(CommandContext ctx, DiscordMessage messageLink)
        {
            if (messageLink.Author != ctx.Guild.CurrentMember)
            {
                await ctx.RespondAsync("That message isn't even mine!");
                return;
            }

            Result<IEnumerable<RoleMenuModel>> options = await _mediator.Send(new GetChannelRoleMenusRequest.Request(ctx.Channel.Id));

            if (!options.Entity.Any())
            {
                await ctx.RespondAsync("There are no role menus in that channel.");
                return;
            }

            RoleMenuModel? selected = options.Entity.FirstOrDefault(x => x.MessageId == messageLink.Id);

            if (selected is null)
            {
                await ctx.RespondAsync("That message isn't a role menu.");
                return;
            }

            try
            {
                await messageLink.DeleteAsync("Role Menu deletion requested.");
            }
            catch
            {
                // ignored
            }

            await ctx.RespondAsync("Poof goes the role-menu! This action cannot be undone!");
            await _mediator.Send(new DeleteRoleMenuRequest.Request(selected));
        }

        private void ResetToMenu(ref DiscordMessage message)
        {
            message = message.ModifyAsync(m => m
                                              .WithContent("Silk! Role Menu Creator v2.0")
                                              .AddComponents(_addFullButton, _addRoleOnlyButton, _editButton)
                                              .AddComponents(_finishButton, _quitButton, _htuButton))
                             .GetAwaiter()
                             .GetResult();
        }

        private record InputResult<T>(bool Cancelled, T? Value);
    }
}