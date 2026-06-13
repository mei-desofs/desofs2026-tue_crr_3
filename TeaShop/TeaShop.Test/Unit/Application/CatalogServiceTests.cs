using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TeaShop.Application.Catalog;

using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using Xunit;

namespace TeaShop.Test.Unit.Application.Catalog;

public class CatalogServiceTests : IDisposable
{
    private readonly ITeaRepository _teaRepositoryMock;
    private readonly CatalogService _sut; 
    private readonly ImageStorageSettings _testSettings;
    private readonly string _testPhysicalStoragePath;

    public CatalogServiceTests()
    {
        _teaRepositoryMock = Substitute.For<ITeaRepository>();

        _testSettings = new ImageStorageSettings
        {
            StoragePath = "TestUploads_" + Guid.NewGuid().ToString(), 
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" },
            AllowedMimeTypes = new[] { "image/jpeg", "image/png" },
            MaxFileSizeInBytes = 1024 * 1024 
        };

        var optionsMock = Options.Create(_testSettings);

        _testPhysicalStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testSettings.StoragePath);

        _sut = new CatalogService(_teaRepositoryMock, optionsMock);
    }

    // Clean up temporary test directories created on the physical OS disk
    public void Dispose()
    {
        if (Directory.Exists(_testPhysicalStoragePath))
        {
            Directory.Delete(_testPhysicalStoragePath, true);
        }
    }

    private IFormFile CreateMockFile(string fileName, string contentType, long sizeBytes)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(sizeBytes);

        file.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return file;
    }



    [Fact]
    public async Task GetByIdAsync_WhenTeaExists_ShouldReturnTeaDto()
    {
        var teaId = Guid.NewGuid();
        var tea = Tea.Create("Matcha", 10.99m, 20, Guid.NewGuid());
        _teaRepositoryMock.GetByIdAsync(teaId, Arg.Any<CancellationToken>()).Returns(tea);

        var result = await _sut.GetByIdAsync(teaId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(tea.Id);
        result.Name.Should().Be(tea.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTeaDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var teaId = Guid.NewGuid();
        _teaRepositoryMock.GetByIdAsync(teaId, Arg.Any<CancellationToken>()).Returns((Tea?)null);

        var act = () => _sut.GetByIdAsync(teaId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Tea not found");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldSaveAndReturnDto()
    {
        var request = new CreateTeaRequestDto("Oolong", 8.50m, 30, Guid.NewGuid());

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);

        await _teaRepositoryMock.Received(1).AddAsync(Arg.Any<Tea>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task UploadImageAsync_WhenTeaDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var teaId = Guid.NewGuid();
        var mockFile = CreateMockFile("test.png", "image/png", 500);
        _teaRepositoryMock.GetByIdWithImagesAsync(teaId, Arg.Any<CancellationToken>()).Returns((Tea?)null);

        var act = () => _sut.UploadImageAsync(teaId, mockFile, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Tea not found.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenFileIsEmpty_ShouldThrowDomainException()
    {
        var tea = Tea.Create("Black Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);

        var emptyFile = CreateMockFile("empty.png", "image/png", 0L); // 0 bytes

        var act = () => _sut.UploadImageAsync(tea.Id, emptyFile, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("File is empty.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenFileSizeExceedsLimit_ShouldThrowDomainException()
    {
        var tea = Tea.Create("Black Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);

        var largeFile = CreateMockFile("large.png", "image/png", _testSettings.MaxFileSizeInBytes + 500);

        var act = () => _sut.UploadImageAsync(tea.Id, largeFile, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("File size exceeds the allowed limit.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenFileExtensionIsInvalid_ShouldThrowDomainException()
    {
        var tea = Tea.Create("Black Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);

        var maliciousFile = CreateMockFile("virus.exe", "image/png", 500L);

        var act = () => _sut.UploadImageAsync(tea.Id, maliciousFile, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Invalid file extension.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenFileMimeTypeIsInvalid_ShouldThrowDomainException()
    {
        var tea = Tea.Create("Black Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);

        var spoofedFile = CreateMockFile("image.png", "text/html", 500L);

        var act = () => _sut.UploadImageAsync(tea.Id, spoofedFile, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Invalid Content-Type.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenValidInputs_ShouldSucceedAndSave()
    {
        var tea = Tea.Create("Black Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);
        var validFile = CreateMockFile("matcha.png", "image/png", 5000L);

        await _sut.UploadImageAsync(tea.Id, validFile, CancellationToken.None);

        tea.Image.Should().NotBeNull();
        tea.Image!.FileName.Should().Be("matcha.png");

        await _teaRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task DeleteImageAsync_WhenNoImageAssociated_ShouldThrowKeyNotFoundException()
    {
        var tea = Tea.Create("Green Tea", 5.00m, 10, Guid.NewGuid());
        _teaRepositoryMock.GetByIdWithImagesAsync(tea.Id, Arg.Any<CancellationToken>()).Returns(tea);

        var act = () => _sut.DeleteImageAsync(tea.Id, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No image associated with this tea.");
    }

   
}