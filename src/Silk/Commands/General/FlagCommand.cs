using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Bot;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.General;


public class FlagCommand : CommandGroup
{
    private readonly FlagOverlayService _flags;
    private readonly ITextCommandContext _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    public FlagCommand(FlagOverlayService flags, ITextCommandContext context, IDiscordRestChannelAPI channels)
    {
        _flags         = flags;
        _context       = context;
        _channels = channels;
    }
    
    
    [Command("flagify")]
    [Description("Add a flag overlay to an image! Upload an image, emoji, or URL.\n\n"                +
                 "Supported flags are: \n`bi[sexual]` \n`trans[gender]` \n`enby`, `nb`, or `nonbinary` " +
                 "\n`ace` or `asexual` \n`demi[sexual]`, \n`mlm` \n`pan[sexual]` \n`lgbtq[+]` or `pride`\n\n")]
    public Task<Result<IMessage>> Flagify
    (
        [Description("The flag to add to the image.")]
        string flag,
        
        [Description("Either an image URL or an emoji to apply the overlay to. \n" +
                     "An image can also be uploaded in lieu of either of these!")]
        OneOf<IPartialEmoji, string>? emojiOrImageUrl = null,
        
        [Option('i', "intensity")]
        [Description("The intensity of the overlay, between 50 and 100.")]
        float intensity = 100,
        
        [Option('g', "grayscale")]
        [Description("How much greyscale to apply to before applying the overlay. " +
                     "Try specifying this if the image doesn't look right.")]
        float grayscale = 0
    )
    {
        if (emojiOrImageUrl is not { })
        {
            if (!_context.Message.Attachments.IsDefined(out var attachments) || !attachments.Any())
                return _channels.CreateMessageAsync(_context.GetChannelID(), $"{Emojis.WarningEmoji} You must specify an image or emoji to apply the overlay to!");
            else 
                return FlagifyImage(flag, attachments.First().Url, intensity, grayscale);
        }
            
        if (!emojiOrImageUrl.Value.TryPickT0(out var emoji, out var imageUrl))
            return FlagifyImage(flag, imageUrl.ToString(), intensity, grayscale);
        
        // unicode emojis have an id of 0, and do not have a link, so we can't use them
        if (!emoji.ID.IsDefined(out var emojiID))
            return _channels.CreateMessageAsync(_context.GetChannelID(),"Unfortunately, unicode emojis do not have a link, and cannot be used. Try uploading an image instead.");

        var emojiLinkResult = CDN.GetEmojiUrl(emojiID.Value, emoji.IsAnimated.IsDefined(out var animated) && animated ? CDNImageFormat.GIF : CDNImageFormat.PNG, 256);
        
        if (!emojiLinkResult.IsSuccess)
            return _channels.CreateMessageAsync(_context.GetChannelID(), "I couldn't find the emoji you specified. Try uploading an image instead.");
        
        return FlagifyImage(flag, emojiLinkResult.Entity.ToString(), intensity, grayscale);
    }
    

    
    //[Cooldown(15, 15, CooldownBucketType.User)]

    public async Task<Result<IMessage>> FlagifyImage(string type, string imageUrl, float intensity = 100, float grayscale = 0)
    {
        if (intensity is < 50 or > 100)
            return await _channels.CreateMessageAsync(_context.GetChannelID(), "Intensity must be between 50 and 100");
        
        if (grayscale is < 0 or > 100)
            return await _channels.CreateMessageAsync(_context.GetChannelID(),"Grayscale must be between 0 and 100");
        
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
            return await _channels.CreateMessageAsync(_context.GetChannelID(), $"{type} is not a flag I seem to have an overlay for yet. Sorry!");
    
        var result = await _flags.GetFlagAsync(imageUrl, overlay.Value, intensity, grayscale);

        if (result.IsSuccess)
        {
            await using var image = result.Entity;
            
            return await _channels.CreateMessageAsync(_context.GetChannelID(), "Here you go!", 
                                                      attachments: new OneOf<FileData, IPartialAttachment>[]
                                                      {
                                                          new FileData("output.png", image, $"Flag type: {overlay}, Intensity: {(intensity * 100):N0}%, Grayscale: {(grayscale * 100):N0}%")
                                                      });
        }
        else
        {
            return await _channels.CreateMessageAsync(_context.GetChannelID(), result.Error.Message);
        }
    }
}