using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Remora.Results;
using Silk.Shared.Constants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Services.Bot;

public enum FlagOverlay
{
    MaleLovingMale,
    Transgender,
    Demisexual,
    NonBinary,
    Bisexual,
    Asexual,
    Pansexual,
    LGBTQPride
}

public sealed class FlagOverlayService
{

    private const int TwoMegaBytes      = 1000 * 1000 * 2;
    private const int MaxImageDimension = 3000;
    
    private readonly HttpClient                  _httpClient;
    private readonly ILogger<FlagOverlayService> _logger;

    public FlagOverlayService(HttpClient httpClient, ILogger<FlagOverlayService> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
    }

    public FlagOverlayService(HttpClient client) : this(client, NullLogger<FlagOverlayService>.Instance) { }

    /// <summary>
    ///     Generates a flag image from the given url.
    /// </summary>
    /// <param name="imageUrl">The url of the image to overlay</param>
    /// <param name="overlay">The overlay to apply</param>
    /// <param name="intensity">The intensity of the overlay to apply with.</param>
    /// <param name="grayscale">The amount of grayscale to apply to the image</param>
    /// <returns>
    ///     A result type that defines whether the operation succeeded, why it did not succeed, and a stream containing the content of the generated
    ///     image.
    /// </returns>
    public async Task<Result<Stream>> GetFlagAsync(string imageUrl, FlagOverlay overlay, float intensity, float grayscale = 0)
    {
        if (intensity is < 0 or > 1)
            return Result<Stream>.FromError(new ArgumentOutOfRangeError(nameof(intensity), "Intensity must be between 0 and 1"));

        if (!Enum.IsDefined(typeof(FlagOverlay), overlay))
            return Result<Stream>.FromError(new ArgumentOutOfRangeError(nameof(overlay), $"Overlay must be specified in {nameof(FlagOverlay)}"));

        var getImage = GetImageUri(imageUrl);

        if (!getImage.IsDefined(out var imageUri))
            return Result<Stream>.FromError(getImage);

        try
        {
            if (!await ValidateImageSizeAsync(imageUrl))
                return Result<Stream>.FromError(new ArgumentOutOfRangeError(nameof(imageUrl), "The image's file size exceeds the 2MB limit."));
        }
        catch { return Result<Stream>.FromError(new NotFoundError("The specified image could not be found.")); }

        _logger.LogDebug(EventIds.Service, "Processing overlay: {OverlayType}", overlay);

        DateTime before = DateTime.UtcNow;

        await using MemoryStream imageStream = await GetImageAsync(imageUri!);

        using Image? image = await Image.LoadAsync(imageStream);

        if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
            return Result<Stream>.FromError(new ArgumentOutOfRangeError(nameof(imageUrl), "The image's dimensions exceed the 3000x3000 limit. Consider resizing the image."));

        Stream overlayImage = await GetOverlayAsync(image, overlay, intensity, grayscale);

        DateTime after = DateTime.UtcNow;

        _logger.LogDebug(EventIds.Service, "Processed overlay in {Time:N1}ms", (after - before).TotalMilliseconds);

        return Result<Stream>.FromSuccess(overlayImage);
    }

    private static async Task<Stream> GetOverlayAsync(Image image, FlagOverlay overlay, float intensity, float grayscale)
    {
        using Image overlaySelection = overlay switch
        {
            FlagOverlay.LGBTQPride     => Image.Load(await File.ReadAllBytesAsync("./flags/pride.png")),
            FlagOverlay.MaleLovingMale => Image.Load(await File.ReadAllBytesAsync("./flags/mlm.png")),
            FlagOverlay.Bisexual       => Image.Load(await File.ReadAllBytesAsync("./flags/bi.png")),
            FlagOverlay.Demisexual     => Image.Load(await File.ReadAllBytesAsync("./flags/demi.png")),
            FlagOverlay.NonBinary      => Image.Load(await File.ReadAllBytesAsync("./flags/enby.png")),
            FlagOverlay.Transgender    => Image.Load(await File.ReadAllBytesAsync("./flags/trans.png")),
            FlagOverlay.Pansexual      => Image.Load(await File.ReadAllBytesAsync("./flags/pan.png")),
            FlagOverlay.Asexual        => Image.Load(await File.ReadAllBytesAsync("./flags/ace.png")),

            _ => throw new ArgumentOutOfRangeException(nameof(overlay), overlay, null)
        };

        using var resizedOverlay = overlaySelection.Clone(m => m.Resize(image.Width, image.Height));
        
        // ReSharper disable once AccessToDisposedClosure
        image.Mutate(x => x.Grayscale(grayscale).DrawImage(resizedOverlay, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcAtop, intensity));

        var stream = new MemoryStream();
        
        await image.SaveAsPngAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        
        return stream;
    }

    private async Task<bool> ValidateImageSizeAsync(string imageUrl)
    {
        // Typically a 'preflight' request is OPTIONS, not HEAD, but we're concerned about the size of the image, so we're using HEAD
        using var preflight = await _httpClient.SendAsync(new(HttpMethod.Head, imageUrl));

        if (preflight.IsSuccessStatusCode) // False if the host doesn't support HEAD requests
        {
            return preflight.Content.Headers.ContentLength < TwoMegaBytes;
        }
        
        _logger.LogTrace(EventIds.Service, "Preflight request failed, falling back to manual fetching.");
        using var secondarySizeRequest = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

        secondarySizeRequest.EnsureSuccessStatusCode();

        return secondarySizeRequest.Content.Headers.ContentLength < TwoMegaBytes;
    }

    private async Task<MemoryStream> GetImageAsync(Uri imageUri) => new(await _httpClient.GetByteArrayAsync(imageUri));

    private static Result<Uri> GetImageUri(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return Result<Uri>.FromError(new ArgumentInvalidError(nameof(imageUrl), "The image URL cannot be null or empty."));

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return Result<Uri>.FromError(new ArgumentInvalidError(nameof(imageUrl), "The specified image URL is not a valid link."));

        if (uri.Scheme != "http" && uri.Scheme != "https")
            return Result<Uri>.FromError(new ArgumentInvalidError(nameof(imageUrl), "The specified image URL is not a valid link."));

        //check that the image ends with an image extension
        if (!imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.Contains(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.Contains(".jpeg", StringComparison.OrdinalIgnoreCase))
            return Result<Uri>.FromError(new ArgumentInvalidError(nameof(imageUrl), "The specified image URL does not point to an image."));
        
        return Result<Uri>.FromSuccess(uri);
    }
}