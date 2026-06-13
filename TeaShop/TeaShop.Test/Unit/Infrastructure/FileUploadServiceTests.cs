using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.IO;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Security;
using Xunit;

namespace TeaShop.Test.Unit.Infrastructure;

public class FileUploadServiceTests : IDisposable
{
    private readonly string _testPhysicalStoragePath;
    private readonly ImageStorageSettings _settings;
    private readonly FileUploadService _sut;

    public FileUploadServiceTests()
    {
        var relativePath = "TestUploads_" + Guid.NewGuid().ToString();

        _settings = new ImageStorageSettings
        {
            StoragePath = relativePath, 
            AllowedExtensions = new[] { ".png", ".jpg" },
            AllowedMimeTypes = new[] { "image/png", "image/jpeg" },
            MaxFileSizeInBytes = 1000
        };

        var optionsMock = Options.Create(_settings);

        _testPhysicalStoragePath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

        _sut = new FileUploadService(optionsMock);
    }

    // Clean up files created during security testing
    public void Dispose()
    {
        if (Directory.Exists(_testPhysicalStoragePath))
        {
            Directory.Delete(_testPhysicalStoragePath, true);
        }
    }

    private IFormFile CreateTestFileStream(string fileName, byte[] content)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.Length.Returns(content.Length);

        file.OpenReadStream().Returns(x => new MemoryStream(content));

        file.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(x => {
                var targetStream = x.Arg<Stream>();
                using var ms = new MemoryStream(content);
                return ms.CopyToAsync(targetStream);
            });

        return file;
    }

    [Fact]
    public async Task SaveFileAsync_WithValidPngAndMatchingMagicBytes_ShouldSaveSuccessfully()
    {
        var validPngContent = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0 };
        var file = CreateTestFileStream("valid.png", validPngContent);

        var savedPath = await _sut.SaveFileAsync(file, CancellationToken.None);

        Assert.True(File.Exists(savedPath));
    }

    [Fact]
    public async Task SaveFileAsync_WithFakePngExtensionButInsecureContent_ShouldThrowDomainException()
    {
 var maliciousContent = System.Text.Encoding.UTF8.GetBytes("echo 'malicious script';");
        var file = CreateTestFileStream("hacked_image.png", maliciousContent);

     await Assert.ThrowsAsync<DomainException>(() =>
            _sut.SaveFileAsync(file, CancellationToken.None));
    }

    [Fact]
    public async Task SaveFileAsync_WhenFileSizeExceedsLimit_ShouldThrowDomainException()
    {
 
        var largeContent = new byte[2000];
        var file = CreateTestFileStream("oversized.png", largeContent);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.SaveFileAsync(file, CancellationToken.None));
    }
}