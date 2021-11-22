using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Silk.Core.Services.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Core.Tests.Services
{
	public class FlagHelperTests
	{

		private readonly Image _enby =
			Image
				.Load(File.ReadAllBytes("./enby.png"))
				.Clone(m => m.Resize(3000, 3000)
					.DrawImage(new Image<Rgba32>(3000, 3000)
						.Clone(m => m.Fill(Color.White)), PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcAtop, 1f));

		private readonly byte[] _enbyImage = File.ReadAllBytes("./enby.png");

		private readonly HttpClient _httpClient;

		private readonly byte[] _imageThatExceedsMaxSize;
		private readonly byte[] _mockArray;
		private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();

		public FlagHelperTests()
		{
			_httpClient = new HttpClient(_mockHttpMessageHandler.Object);

			// Set up a blank white image so multiplying will result in the source image
			using var stream = new MemoryStream();
			using var image = new Image<Rgba32>(3000, 3000);
			image.Mutate(x => x.Fill(Color.White));

			image.SaveAsPng(stream);
			stream.Position = 0;
			_mockArray = stream.ToArray();

			// Set up an image that exceeds the max size
			using var stream2 = new MemoryStream();

			using var imageThatExceedsMaxSize = new Image<Rgba32>(3001, 3001);
			imageThatExceedsMaxSize.Mutate(x => x.Fill(Color.White));

			imageThatExceedsMaxSize.SaveAsPng(stream2);
			stream2.Position = 0;
			_imageThatExceedsMaxSize = stream2.ToArray();

			using var stream3 = new MemoryStream();
			_enby.SaveAsPng(stream3);
		}

		[Test]
		public void GetFlagAsync_Throws_When_Intensity_IsInvalid()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await flagHelper.GetFlagAsync("", 0, 2));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("intensity"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Flag_IsInvalid()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await flagHelper.GetFlagAsync("", (FlagOverlay)int.MaxValue, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("overlay"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_IsNull()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await flagHelper.GetFlagAsync(null!, FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_IsEmpty()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await flagHelper.GetFlagAsync("", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_DoesNot_HaveDomain()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await flagHelper.GetFlagAsync("https://", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_DoesNot_HaveExtension()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await flagHelper.GetFlagAsync("https://example.com", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_IsNot_Https()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await flagHelper.GetFlagAsync("file://C:/somewhere/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}

		[Test]
		public void GetFlagAsync_Throws_When_Url_IsNot_Image()
		{
			// Arrange
			var flagHelper = new FlagOverlayService(_httpClient);

			// Act
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await flagHelper.GetFlagAsync("https://example.com/image.txt", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.That(exception?.ParamName, Is.EqualTo("imageUrl"));
		}


		[Test]
		public async Task GetFlagAsync_Preflight_OnlyUses_HEAD_When_ContentLength_IsReturned()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") { Headers = { ContentLength = int.MaxValue } } });
			//.ReturnsAsync();

			// Act
			Assert.DoesNotThrowAsync(async () => await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			mockHttpMessageHandler
				.Protected()
				.Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
		}

		[Test]
		public async Task GetFlagAsync_Preflight_Uses_GET_When_ContentLengh_NotReturned()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") { Headers = { ContentLength = int.MaxValue } } });


			// Act
			Assert.DoesNotThrowAsync(async () => await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			mockHttpMessageHandler
				.Protected()
				.Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
		}

		[Test]
		public void GetFlagAsync_Returns_FileNotFound_When_ContentLength_IsNotReturned()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

			// Act
			FlagResult? result = null;
			Assert.DoesNotThrowAsync(async () => result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
			Assert.AreEqual(FlagResultType.FileNotFound, result.Reason);
		}

		[Test]
		public void GetFlagAsync_Returns_FileSizeTooLarge_When_ContentLength_BiggerThan2MB()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") { Headers = { ContentLength = int.MaxValue } } });

			// Act
			FlagResult? result = null;
			Assert.DoesNotThrowAsync(async () => result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
			Assert.AreEqual(FlagResultType.FileSizeTooLarge, result.Reason);
		}

		[Test]
		public async Task GetFlagAsync_Returns_FileDimensionsTooLarge_When_ImageDimensions_Exceed_3000px()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_imageThatExceedsMaxSize) { Headers = { ContentLength = _imageThatExceedsMaxSize.Length } } });

			// Act
			FlagResult? result = null;
			Assert.DoesNotThrowAsync(async () => result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0));

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
			Assert.AreEqual(FlagResultType.FileDimensionsTooLarge, result.Reason);
		}

		[Test]
		public void GetFlagAsync_Returns_SuccessfulResult_When_ImageDimensions_DoesNotExceed_3000px()
		{
			// Arrange
			var buffer = new byte[_mockArray.Length];

			var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var flagHelper = new FlagOverlayService(httpClient);

			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new MemoryStream(_mockArray)) { Headers = { ContentLength = _mockArray.Length } } });

			// Act
			FlagResult? result = null;
			Assert.DoesNotThrowAsync(async () => result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 1));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Succeeded);
			Assert.AreEqual(FlagResultType.Succeeded, result.Reason);

			var expected = Image.Load(_enbyImage);
			var actual = Image.Load<Rgba32>(result.Image);

			for (int i = 0; i < expected.Height; i++)
			{
				var e = expected.GetPixelRowSpan(i);
				var a = actual.GetPixelRowSpan(i);

				Assert.True(e.SequenceEqual(a));
			}
		}

		public interface IHttpMethodHandler
		{
			Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
		}
	}
}