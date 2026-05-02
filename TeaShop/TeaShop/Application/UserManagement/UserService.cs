using TeaShop.Application.Auth;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Application.UserManagement;

public sealed class UserService
{
    private readonly IUserRepository _users;
    private readonly PasswordHashingService _hasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository users, PasswordHashingService hasher, ILogger<UserService> logger)
    {
        _users = users;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<AddressResponse?> GetAddressAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var user = await _users.FindByIdAsync(userId, ct)
                ?? throw new NotFoundException(FailureMessages.User.NotFound);

            if (user.ShippingAddress is null)
                return null;

            return new AddressResponse(
                user.ShippingAddress.Street,
                user.ShippingAddress.City,
                user.ShippingAddress.PostalCode,
                user.ShippingAddress.Country);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while retrieving address for user. Id: {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateAddressAsync(Guid userId, UpdateAddressRequest req, CancellationToken ct)
    {
        try
        {
            var user = await _users.FindByIdAsync(userId, ct)
                ?? throw new NotFoundException(FailureMessages.User.NotFound);

            var newAddress = Address.Create(req.Street, req.City, req.PostalCode, req.Country);

            user.UpdateShippingAddress(newAddress);

            await _users.SaveChangesAsync(ct);

            _logger.LogInformation("Address updated for user. Id: {UserId}", userId);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while updating address for user. Id: {UserId}", userId);
            throw;
        }
    }

    public async Task RemoveAddressAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var user = await _users.FindByIdAsync(userId, ct)
                ?? throw new NotFoundException(FailureMessages.User.NotFound);

            if (user.ShippingAddress is null)
                throw new NotFoundException(FailureMessages.User.AddressNotFound);

            user.RemoveShippingAddress();

            await _users.SaveChangesAsync(ct);

            _logger.LogInformation("Address removed for user. Id: {UserId}", userId);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while removing address for user. Id: {UserId}", userId);
            throw;
        }
    }

    public async Task<StaffCreatedResponse> CreateStaffAsync(CreateStaffRequest req, CancellationToken ct)
    {
        try
        {
            if (await _users.ExistsByEmailAsync(req.Email, ct))
                throw new ConflictException(FailureMessages.User.EmailAlreadyExists);

            var user = User.CreateStaff(req.Email, _hasher.Hash(req.Password), req.Role);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            var sanitizedRoleForLog = (user.Role ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            _logger.LogWarning(
                "Staff account created. NewUserId: {NewUserId}, Role: {Role}",
                user.Id, sanitizedRoleForLog);

            return new StaffCreatedResponse(user.Id, user.Email.Value, user.Role);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while creating staff account.");
            throw;
        }
    }
}
