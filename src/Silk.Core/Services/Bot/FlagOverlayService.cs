using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silk.Shared.Constants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Core.Services.Bot;

public sealed record FlagResult(bool Succeeded, FlagResultType Reason, Stream? Image);

public enum FlagResultType
{
    FileDimensionsTooLarge,
    FileSizeTooLarge,
    FileNotImage,
    FileNotFound,
    Succeeded,
}

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

    private static readonly Image _prideImage = Image.Load(File.ReadAllBytes("./flags/pride.png"));
    private static readonly Image _transImage = Image.Load(File.ReadAllBytes("./flags/trans.png"));
    private static readonly Image _demiImage  = Image.Load(File.ReadAllBytes("./flags/demi.png"));
    private static readonly Image _enbyImage  = Image.Load(File.ReadAllBytes("./flags/enby.png"));
    private static readonly Image _panImage   = Image.Load(File.ReadAllBytes("./flags/pan.png"));
    private static readonly Image _aceImage   = Image.Load(File.ReadAllBytes("./flags/ace.png"));
    private static readonly Image _mlmImage   = Image.Load(File.ReadAllBytes("./flags/mlm.png"));
    private static readonly Image _biImage    = Image.Load(File.ReadAllBytes("./flags/bi.png"));

    private readonly HttpClient                  _httpClient;
    private readonly ILogger<FlagOverlayService> _logger;

    public FlagOverlayService(HttpClient httpClient, ILogger<FlagOverlayService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
    public async Task<FlagResult> GetFlagAsync(string imageUrl, FlagOverlay overlay, float intensity, float grayscale = 0)
    {
        OverlayGuard.ValidIntensity(intensity);

        OverlayGuard.ValidFlagOverlay(overlay);

        OverlayGuard.ValidImageUrl(imageUrl, out Uri? imageUri);

        try
        {
            if (!await ValidateImageSizeAsync(imageUrl))
                return new(false, FlagResultType.FileSizeTooLarge, null);
        }
        catch { return new(false, FlagResultType.FileNotFound, null); }

        _logger.LogDebug(EventIds.Service, "Processing overlay: {OverlayType}", overlay);

        DateTime before = DateTime.UtcNow;

        await using MemoryStream imageStream = await GetImageAsync(imageUri!);

        using Image? image = await Image.LoadAsync(imageStream);

        if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
            return new(false, FlagResultType.FileDimensionsTooLarge, null);

        Stream? overlayImage = await GetOverlayAsync(image, overlay, intensity, grayscale);

        DateTime after = DateTime.UtcNow;

        _logger.LogDebug(EventIds.Service, "Processed overlay in {Time:N1}ms", (after - before).TotalMilliseconds);

        return new(true, FlagResultType.Succeeded, overlayImage);
    }

    private static async Task<Stream> GetOverlayAsync(Image image, FlagOverlay overlay, float intensity, float grayscale)
    {
        Image? overlaySelection = overlay switch
        {
            FlagOverlay.LGBTQPride     => _prideImage,
            FlagOverlay.MaleLovingMale => _mlmImage,
            FlagOverlay.Bisexual       => _biImage,
            FlagOverlay.Demisexual     => _demiImage,
            FlagOverlay.NonBinary      => _enbyImage,
            FlagOverlay.Transgender    => _transImage,
            FlagOverlay.Pansexual      => _panImage,
            FlagOverlay.Asexual        => _aceImage,

            _ => throw new ArgumentOutOfRangeException(nameof(overlay), overlay, null)
        };

        using Image? resizedOverlay = overlaySelection.Clone(m => m.Resize(image.Width, image.Height));

        image.Mutate(x => x.Grayscale(grayscale).DrawImage(resizedOverlay, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcAtop, intensity));

        var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        return stream;
    }

    private async Task<bool> ValidateImageSizeAsync(string imageUrl)
    {
        // Typically a 'preflight' request is OPTIONS, not HEAD, but we're concerned about the size of the image, so we're using HEAD
        using HttpResponseMessage? preflight = await _httpClient.SendAsync(new(HttpMethod.Head, imageUrl));

        if (preflight.IsSuccessStatusCode) // False if the host doesn't support HEAD requests
        {
            return preflight.Content.Headers.ContentLength < TwoMegaBytes;
        }
        _logger.LogTrace(EventIds.Service, "Preflight request failed, falling back to manual fetching.");
        using HttpResponseMessage? secondarySizeRequest = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

        secondarySizeRequest.EnsureSuccessStatusCode();

        return secondarySizeRequest.Content.Headers.ContentLength < TwoMegaBytes;
    }

    private async Task<MemoryStream> GetImageAsync(Uri imageUri)
    {
        return new(await _httpClient.GetByteArrayAsync(imageUri));
    }

    private static class OverlayGuard
    {
        public static void ValidImageUrl(string imageUrl, out Uri? uri)
        {
            if (imageUrl is null)
                throw new ArgumentNullException(nameof(imageUrl));

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out uri))
                throw new ArgumentException("Invalid image url", nameof(imageUrl));

            if (uri.Scheme != "http" && uri.Scheme != "https")
                throw new ArgumentException("Invalid image uri scheme", nameof(imageUrl));

            //check that the image ends with an image extension
            if (!imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase) &&
                !imageUrl.Contains(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !imageUrl.Contains(".jpeg", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid image url", nameof(imageUrl));
        }

        public static void ValidIntensity(float intensity)
        {
            if (intensity < 0 || intensity > 1)
                throw new ArgumentOutOfRangeException(nameof(intensity));
        }

        public static void ValidFlagOverlay(FlagOverlay overlay)
        {
            if (!Enum.IsDefined(typeof(FlagOverlay), overlay))
                throw new ArgumentOutOfRangeException(nameof(overlay));
        }
    }
}