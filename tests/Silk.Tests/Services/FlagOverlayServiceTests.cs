using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Remora.Results;
using Silk.Services.Bot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Tests.Services;

public class FlagOverlayServiceTests
{
    private readonly Image<Rgba32> _enby = Image.Load(File.ReadAllBytes("./flags/enby.png"));

    private readonly HttpClient _httpClient;

    private readonly byte[]                   _imageThatExceedsMaxSize;
    private readonly byte[]                   _mockArray;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();

    public FlagOverlayServiceTests()
    {
        _httpClient = new(_mockHttpMessageHandler.Object);

        // Set up a blank white image so multiplying will result in the source image
        using var stream = new MemoryStream();
        using var image  = new Image<Rgba32>(3000, 3000);
        image.Mutate(x => x.Fill(Color.White));

        image.SaveAsPng(stream);
        stream.Position = 0;
        _mockArray      = stream.ToArray();

        // Set up an image that exceeds the max size
        using var stream2 = new MemoryStream();

        using var imageThatExceedsMaxSize = new Image<Rgba32>(3001, 3001);
        imageThatExceedsMaxSize.Mutate(x => x.Fill(Color.White));

        imageThatExceedsMaxSize.SaveAsPng(stream2);
        stream2.Position         = 0;
        _imageThatExceedsMaxSize = stream2.ToArray();

        _ = _enby;
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentOutOfRangeError_When_Intensity_IsInvalid()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("", 0, 2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentOutOfRangeError>(result.Error);
        Assert.AreEqual("intensity", (result.Error as ArgumentOutOfRangeError)!.Name);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentOutOfRangeError_When_Flag_IsInvalid()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("", (FlagOverlay)int.MaxValue, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentOutOfRangeError>(result.Error);
        Assert.AreEqual("overlay", (result.Error as ArgumentOutOfRangeError)!.Name);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_IsNull()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync(null!, FlagOverlay.NonBinary, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
        Assert.AreEqual("Error in argument imageUrl: The image URL cannot be null or empty.", result.Error !.Message);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_IsEmpty()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("", FlagOverlay.NonBinary, 0);

        var ane = result.Error as ArgumentInvalidError;
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
        
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_DoesNot_HaveDomain()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("https://", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_DoesNot_HaveExtension()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_IsNot_Https()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("file://C:/somewhere/image.png", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
        Assert.AreEqual("Error in argument imageUrl: The specified image URL is not a valid link.", result.Error!.Message);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentInvalidError_When_Url_IsNot_Image()
    {
        // Arrange
        var flagHelper = new FlagOverlayService(_httpClient);

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com/image.txt", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentInvalidError>(result.Error);
        Assert.AreEqual("imageUrl", (result.Error as ArgumentInvalidError)!.Name);
        Assert.AreEqual("Error in argument imageUrl: The specified image URL does not point to an image.", result.Error!.Message);
    }


    [Test]
    public async Task GetFlagAsync_Preflight_OnlyUses_HEAD_When_ContentLength_IsReturned()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

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
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

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
    public async Task GetFlagAsync_Returns_NotFoundError_When_ContentLength_IsNotReturned()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

        var flagHelper = new FlagOverlayService(httpClient);

        mockHttpMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<NotFoundError>(result.Error);
        Assert.AreEqual("The specified image could not be found.", result.Error!.Message);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentOutOfRangeError_When_Image_Is_BiggerThan2MB()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

        var flagHelper = new FlagOverlayService(httpClient);

        mockHttpMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") { Headers = { ContentLength = int.MaxValue } } });

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentOutOfRangeError>(result.Error);
        Assert.AreEqual("Error in argument imageUrl: The image's file size exceeds the 2MB limit.", result.Error!.Message);
    }

    [Test]
    public async Task GetFlagAsync_Returns_ArgumentOutOfRangeError_When_ImageDimensions_Exceed_3000px()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

        var flagHelper = new FlagOverlayService(httpClient);

        Func<HttpResponseMessage> response = () => new(HttpStatusCode.OK) { Content = new ByteArrayContent(_imageThatExceedsMaxSize) { Headers = { ContentLength = _imageThatExceedsMaxSize.Length } } };

        mockHttpMessageHandler
           .Protected()
           .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(response)  // We perform a preflight request to get the content length, and then we perform the actual request
           .ReturnsAsync(response); // Since the request gets disposed, we need to re-create it

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 0);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.IsInstanceOf<ArgumentOutOfRangeError>(result.Error);
        Assert.AreEqual("Error in argument imageUrl: The image's dimensions exceed the 3000x3000 limit. Consider resizing the image.", result.Error!.Message);
    }

    [Test]
    public async Task GetFlagAsync_Returns_Success_When_ImageDimensions_DoesNotExceed_3000px()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient             = new HttpClient(mockHttpMessageHandler.Object);

        var flagHelper = new FlagOverlayService(httpClient);

        mockHttpMessageHandler
           .Protected()
           .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") { Headers                           = { ContentLength = _mockArray.Length } } })
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new MemoryStream(_mockArray)) { Headers = { ContentLength = _mockArray.Length } } });

        // Act
        var result = await flagHelper.GetFlagAsync("https://example.com/image.png", FlagOverlay.NonBinary, 1);

        // Assert
        Assert.True(result.IsSuccess);

        Image<Rgba32> expected = _enby;
        Image<Rgba32> actual   = Image.Load<Rgba32>(result.Entity);

        for (var i = 0; i < expected.Height; i++)
        {
            var e = expected.GetPixelRowSpan(i).ToArray();
            var a = actual.GetPixelRowSpan(i).ToArray();

            if (e.Length != a.Length)
                Assert.Fail($"Pixel row in expected differs from actual. Expected: {e.Length} Actual: {a.Length}");
            
            for (var j = 0; j < e.Length; j++)
                if (e[j] != a[j])
                    Assert.Fail($"Pixel row in expected differs from actual at index [{i}, {j}]. Expected: {e[j]} Actual: {a[j]}");
        }
    }
}