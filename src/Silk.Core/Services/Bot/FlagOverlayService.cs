using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silk.Shared.Constants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Core.Services.Bot
{
	public sealed record FlagResult(bool Succeeded, FlagResultType Reason, Stream? Image);

	public enum FlagResultType
	{
		FileDimentionsTooLarge,
		FileSizeTooLarge,
		FileNotImage,
		FileNotFound,
		Succeeded,
	}

	public enum FlagOverlay
	{
		Transgender,
		Demisexual,
		NonBinary,
		Bisexual,
		Pansexual
	}

	public sealed class FlagOverlayService
	{

		private const int TwoMegaBytes = 1000 * 1000 * 2;
		private const int MaxImageDimension = 3000;
		private static readonly Image _transImage = Image.Load(File.ReadAllBytes("./trans.png"));
		private static readonly Image _demiImage = Image.Load(File.ReadAllBytes("./demi.png"));
		private static readonly Image _enbyImage = Image.Load(File.ReadAllBytes("./enby.png"));
		private static readonly Image _panImage = Image.Load(File.ReadAllBytes("./pan.png"));
		private static readonly Image _biImage = Image.Load(File.ReadAllBytes("./bi.png"));

		private readonly HttpClient _httpClient;
		private readonly ILogger<FlagOverlayService> _logger;

		public FlagOverlayService(HttpClient httpClient, ILogger<FlagOverlayService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		/// <summary>
		/// Generates a flag image from the given url.
		/// </summary>
		/// <param name="imageUrl">The url of the image to overlay</param>
		/// <param name="overlay">The overlay to apply</param>
		/// <param name="intensity">The intensity of the overlay to apply with.</param>
		/// <returns>A result type that defines whether the operation succeeded, why it did not succeed, and a stream containing the content of the generated image.</returns>
		public async Task<FlagResult> GetFlagAsync(string imageUrl, FlagOverlay overlay, float intensity)
		{
			OverlayGaurd.ValidIntensity(intensity);

			OverlayGaurd.ValidFlagOverlay(overlay);

			OverlayGaurd.ValidImageUrl(imageUrl, out Uri? imageUri);

			try
			{
				if (!await ValidateImageSizeAsync(imageUrl))
					return new FlagResult(false, FlagResultType.FileSizeTooLarge, null);
			}
			catch { return new FlagResult(false, FlagResultType.FileNotFound, null); }

			_logger.LogDebug(EventIds.Service, "Processing overlay: {OverlayType}", overlay);

			var before = DateTime.UtcNow;

			MemoryStream imageStream = await GetImageAsync(imageUri!);

			var image = await Image.LoadAsync(imageStream);

			if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
				return new FlagResult(false, FlagResultType.FileDimentionsTooLarge, null);

			var overlayImage = await GetOverlayAsync(image, overlay, intensity);

			var after = DateTime.UtcNow;

			_logger.LogDebug(EventIds.Service, "Processed overlay in {Time:N1}ms", (after - before).TotalMilliseconds);

			return new FlagResult(true, FlagResultType.Succeeded, overlayImage);
		}

		private async Task<Stream> GetOverlayAsync(Image image, FlagOverlay overlay, float intensity)
		{
			var overlaySelection = overlay switch
			{
				FlagOverlay.Bisexual => _biImage,
				FlagOverlay.Demisexual => _demiImage,
				FlagOverlay.NonBinary => _enbyImage,
				FlagOverlay.Transgender => _transImage,
				FlagOverlay.Pansexual => _panImage,

				_ => throw new ArgumentOutOfRangeException(nameof(overlay), overlay, null)
			};

			using var resizedOverlay = overlaySelection.Clone(m => m.Resize(image.Width, image.Height));

			using var result = image.CloneAs<Rgba32>();

			result.Mutate(x => x.DrawImage(resizedOverlay, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcAtop, intensity));

			var stream = new MemoryStream();
			await result.SaveAsPngAsync(stream);
			stream.Position = 0;
			return stream;
		}

		private async Task<bool> ValidateImageSizeAsync(string imageUrl)
		{
			// Typically a 'preflight' request is OPTIONS, not HEAD, but we're concerned about the size of the image, so we're using HEAD
			var preflight = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, imageUrl));

			if (preflight.IsSuccessStatusCode) // False if the host doesn't support HEAD requests
			{
				return preflight.Content.Headers.ContentLength < TwoMegaBytes;
			}
			else
			{
				_logger.LogTrace(EventIds.Service, "Preflight request failed, falling back to manual fetching.");
				preflight = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

				if (preflight.IsSuccessStatusCode)
					return preflight.Content.Headers.ContentLength < TwoMegaBytes;

				preflight.EnsureSuccessStatusCode();
				return false; // This is unreachable, but the compiler doesn't know that
			}
		}

		private async Task<MemoryStream> GetImageAsync(Uri imageUri) => new(await _httpClient.GetByteArrayAsync(imageUri));

		private static class OverlayGaurd
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
}