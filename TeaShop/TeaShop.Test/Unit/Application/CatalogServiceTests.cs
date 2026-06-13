using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using TeaShop.Application.Catalog;

using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security.Interfaces;
using Xunit;

namespace TeaShop.Test.Unit.Application.Catalog;

public class CatalogServiceTests
{
    private readonly ITeaRepository _teaRepositoryMock;
    private readonly CatalogService _sut;
    private readonly IFileUploadService _fileUploadServiceMock;

    public CatalogServiceTests()
    {
        _teaRepositoryMock = Substitute.For<ITeaRepository>();
        _fileUploadServiceMock = Substitute.For<IFileUploadService>(); // 2. Instantiate the mock

        _sut = new CatalogService(_teaRepositoryMock, _fileUploadServiceMock);
    }

    private IFormFile CreateMockFile(string fileName, string contentType, long sizeBytes)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(sizeBytes);
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
    public async Task UploadImageAsync_WhenTeaExists_ShouldUploadAndSave()
    {
        var teaId = Guid.NewGuid();
        var file = CreateMockFile("tea.png", "image/png", 500);
        var tea = Tea.Create("Black Tea", 5.00m, 10, teaId);

        _teaRepositoryMock.GetByIdWithImagesAsync(teaId, Arg.Any<CancellationToken>())
            .Returns(tea);

        _fileUploadServiceMock.SaveFileAsync(file, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("/SecureUploadedImages/fake-image-id.png"));

        await _sut.UploadImageAsync(teaId, file, CancellationToken.None);

        await _fileUploadServiceMock.Received(1).SaveFileAsync(file, Arg.Any<CancellationToken>());

        await _teaRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task GetImageAsync_WhenImageExists_ShouldRetrieveFileBytesAndDetails()
    {
        var teaId = Guid.NewGuid();
        var tea = Tea.Create("Black Tea", 5.00m, 10, teaId);
        var fakeFilePath = "/SecureUploadedImages/greentea.png";
        var expectedBytes = new byte[] { 10, 20, 30, 40 };

        tea.SetImage("greentea.png", fakeFilePath, 500); // Associate an image to the tea entity

        _teaRepositoryMock.GetByIdWithImagesAsync(teaId, Arg.Any<CancellationToken>())
            .Returns(tea);

        _fileUploadServiceMock.ReadFileAsync(fakeFilePath, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBytes));

        var result = await _sut.GetImageAsync(teaId, CancellationToken.None);

        Assert.Equal(expectedBytes, result.FileBytes);
        Assert.Equal("image/png", result.MimeType);
        Assert.Equal("greentea.png", result.OriginalName);

        await _fileUploadServiceMock.Received(1).ReadFileAsync(fakeFilePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteImageAsync_WhenImageExists_ShouldDeleteFileAndRemoveFromEntity()
    {
        var teaId = Guid.NewGuid();
        var filePathToDelete = "/SecureUploadedImages/old-image.jpg";

        var tea = Tea.Create("Black Tea", 5.00m, 10, teaId);
        tea.SetImage("old-image.jpg", filePathToDelete, 800);

        _teaRepositoryMock.GetByIdWithImagesAsync(teaId, Arg.Any<CancellationToken>())
            .Returns(tea);

        await _sut.DeleteImageAsync(teaId, CancellationToken.None);

      _fileUploadServiceMock.Received(1).DeleteFile(filePathToDelete);

        Assert.Null(tea.Image);

        await _teaRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }


}