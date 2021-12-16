using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Services.Bot;

namespace Silk.Commands.General;

public class FlagCommand : BaseCommandModule
{
    private readonly FlagOverlayService _flags;
    public FlagCommand(FlagOverlayService flags) => _flags = flags;

    [Command]
    [Priority(0)]
    [Cooldown(15, 15, CooldownBucketType.User)]
    [Description("Add a flag overlay to an image! Upload an image, emoji, or URL.\n\n"      +
                 "Valid flags are: \n"                                                      +
                 "`bi[sexual]`\n"                                                           +
                 "`trans[gender]`\n"                                                        +
                 "`enby` or `nb`, \n"                                                       +
                 "`ace` or `asexual` \n"                                                    +
                 "`demi[sexual]`, \n"                                                       +
                 "`mlm` \n"                                                                 +
                 "`pan[sexual]`\n"                                                          +
                 "`lgbtq[+]` or `pride`\n\n"                                                +
                 "Intensity can be specified as an extra parameter, between 50 and 100. \n" +
                 "Defaults to 100.\n\n"                                                     +
                 "Grayscale can be used if an image doesn't seem to turn out too well.\n"   +
                 "Larger number = grayer background. Max 100, defaults to 0.")]
    public async Task Flagify(CommandContext ctx, string type, string image, float intensity = 100, float grayscale = 0)
    {
        if (intensity is < 50 or > 100)
        {
            await ctx.RespondAsync("Intensity must be between 50 and 100");
            return;
        }

        if (grayscale is < 0 or > 100)
        {
            await ctx.RespondAsync("Grayscale must be between 0 and 100");
            return;
        }

        intensity /= 100;
        grayscale /= 100;

        FlagOverlay? overlay;

        overlay = type.ToLower() switch
        {
            "bi" or "bisexual"             => FlagOverlay.Bisexual,
            "trans" or "transgender"       => FlagOverlay.Transgender,
            "nb" or "enby"                 => FlagOverlay.NonBinary,
            "demi" or "demisexual"         => FlagOverlay.Demisexual,
            "pan" or "pansexual"           => FlagOverlay.Pansexual,
            "ace" or "asexual"             => FlagOverlay.Asexual,
            "mlm"                          => FlagOverlay.MaleLovingMale,
            "pride" or "lgbtq" or "lgtbq+" => FlagOverlay.LGBTQPride,
            _                              => null
        };

        if (!overlay.HasValue)
        {
            await ctx.RespondAsync($"{type} is not a flag I seem to have an overlay for yet. Sorry!");
            return;
        }
        try
        {
            FlagResult? result = await _flags.GetFlagAsync(image, overlay.Value, intensity, grayscale);

            if (result.Succeeded)
            {
                await ctx.RespondAsync(m => m.WithContent("Here you go!").WithFile("output.png", result.Image));

                await result.Image!.DisposeAsync();
            }
            else
            {
                string? message = result.Reason switch
                {
                    FlagResultType.FileNotFound           => "It seems that image has gone missing. Try a different link?",
                    FlagResultType.FileNotImage           => "It...Appears that isn't an image. Sorry!",
                    FlagResultType.FileDimensionsTooLarge => "That file is huge! I can only handle image 2000px x 2000px and smaller.",
                    FlagResultType.FileSizeTooLarge       => "That file appears to be too large. Max file size is 2MB.",
                    _                                     => $"Unknown error `{result.Reason}`"
                };
                await ctx.RespondAsync(message);
            }
        }
        catch
        {
            await ctx.RespondAsync("Something appears to be wrong in the url. Are you sure it's correct? (check for spaces!)");
        }
    }

    [Command]
    [Priority(2)]
    public async Task Flagify(CommandContext ctx, string flag, DiscordEmoji emoji, float intensity = 100, float grayscale = 0)
    {
        // unicode emojis have an id of 0, and do not have a link, so we can't use them
        if (emoji.Id is 0)
        {
            await ctx.RespondAsync("Unfortuantely, unicode emojis do not have a link, and cannot be used. Try uploading an image instead.");
            return;
        }

        await Flagify(ctx, flag, (emoji.Url + "?size=1024"), intensity, grayscale);
    }

    [Command]
    [Priority(1)]
    public async Task Flagify(CommandContext ctx, string flag, float intensity = 100, float grayscale = 0)
    {
        if (ctx.Message.Attachments.Count is 0)
            await ctx.RespondAsync("Please upload an image to use this command.");
        else
            await Flagify(ctx, flag, ctx.Message.Attachments[0].Url, intensity, grayscale);
    }
}