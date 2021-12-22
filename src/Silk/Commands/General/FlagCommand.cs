using System.ComponentModel;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Bot;

namespace Silk.Commands.General;

[ExcludeFromSlashCommands]
public class FlagCommand : CommandGroup
{
    private readonly FlagOverlayService _flags;
    private readonly ICommandContext _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    public FlagCommand(FlagOverlayService flags, ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _flags         = flags;
        _context       = context;
        _channels = channels;
    }
    
    [Command("flagify")]
    [Description("Add a flag overlay to an image! Upload an image, emoji, or URL.\n\n"                +
                 "Valid flags are: \n`bi[sexual]` \n`trans[gender]` \n`enby`, `nb`, or `nonbinary` " +
                 "\n`ace` or `asexual` \n`demi[sexual]`, \n`mlm` \n`pan[sexual]` \n`lgbtq[+]` or `pride`\n\n")]
    public Task<Result<IMessage>> Flagify(
        [Description("The flag to add to the image.")]
        string flag,
        [Description("Either an image URL or an emoji to apply the overlay to.")]
        OneOf<IPartialEmoji, string> emojiOrImageUrl,
        [Description("The intensity of the overlay, between 50 and 100.")]
        float intensity = 100,
        [Description("How much greyscale to apply to before applying the overlay. " +
                     "Try specifying this if the image doesn't look right.")]
        float grayscale = 0)
    {
        if (!emojiOrImageUrl.TryPickT0(out var emoji, out var imageUrl))
            return Flagify(flag, imageUrl, intensity, grayscale);
        
        // unicode emojis have an id of 0, and do not have a link, so we can't use them
        if (!emoji.ID.IsDefined(out var emojiID))
            return _channels.CreateMessageAsync(_context.ChannelID,"Unfortuantely, unicode emojis do not have a link, and cannot be used. Try uploading an image instead.");

        var emojiLinkResult = CDN.GetEmojiUrl(emojiID.Value, (emoji.IsAnimated.IsDefined(out var animated) && animated) ? CDNImageFormat.GIF : CDNImageFormat.PNG, 256);
        
        if (!emojiLinkResult.IsSuccess)
            return _channels.CreateMessageAsync(_context.ChannelID, "I couldn't find the emoji you specified. Try uploading an image instead.");
        
        return Flagify(flag, emojiLinkResult.Entity.ToString(), intensity, grayscale);
    }
    

    
    //[Cooldown(15, 15, CooldownBucketType.User)]

    public async Task<Result<IMessage>> Flagify(string type, string imageUrl, float intensity = 100, float grayscale = 0)
    {
        if (intensity is < 50 or > 100)
            return await _channels.CreateMessageAsync(_context.ChannelID, "Intensity must be between 50 and 100");
        
        if (grayscale is < 0 or > 100)
            return await _channels.CreateMessageAsync(_context.ChannelID,"Grayscale must be between 0 and 100");
        
        intensity /= 100;
        grayscale /= 100;

        FlagOverlay? overlay;

        overlay = type.ToLower() switch
        {
            "ace" or "asexual"             => FlagOverlay.Asexual,
            "bi" or "bisexual"             => FlagOverlay.Bisexual,
            "pan" or "pansexual"           => FlagOverlay.Pansexual,
            "nb" or "enby" or "nonbinary"  => FlagOverlay.NonBinary,
            "demi" or "demisexual"         => FlagOverlay.Demisexual,
            "pride" or "lgbtq" or "lgtbq+" => FlagOverlay.LGBTQPride,
            "trans" or "transgender"       => FlagOverlay.Transgender,
            "mlm"                          => FlagOverlay.MaleLovingMale,
            _                              => null
        };

        if (!overlay.HasValue)
            return await _channels.CreateMessageAsync(_context.ChannelID,$"{type} is not a flag I seem to have an overlay for yet. Sorry!");
    
        var result = await _flags.GetFlagAsync(imageUrl, overlay.Value, intensity, grayscale);

        if (result.IsSuccess)
        {
            await using var image = result.Entity;
            
            return await _channels.CreateMessageAsync(_context.ChannelID, "Here you go!", 
                                                      attachments: new OneOf<FileData, IPartialAttachment>[]
                                                      {
                                                          new FileData("output.png", image, $"Flag type: {overlay}, Intensity: {(intensity * 100):N0}%, Grayscale: {(grayscale * 100):N0}%")
                                                      });
        }
        else
        {
            return await _channels.CreateMessageAsync(_context.ChannelID, result.Error.Message);
        }
    }


    /*
        [Command]
        [Priority(1)]
        public async Task Flagify(CommandContext ctx, string flag, float intensity = 100, float grayscale = 0)
        {
            if (ctx.Message.Attachments.Count is 0)
                await ctx.RespondAsync("Please upload an image to use this command.");
            else
                await Flagify(ctx, flag, ctx.Message.Attachments[0].Url, intensity, grayscale);
        }*/
}