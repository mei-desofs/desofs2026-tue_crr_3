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

    public async Task UpdateAddressAsync(Guid userId, UpdateAddressRequest req, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        var newAddress = Address.Create(req.Street, req.City, req.PostalCode, req.Country);

        user.UpdateShippingAddress(newAddress);

        await _users.SaveChangesAsync(ct);
    }

    public async Task<StaffCreatedResponse> CreateStaffAsync(
        CreateStaffRequest req, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(req.Email, ct))
            throw new ConflictException("A user with this email already exists.");

        var user = User.CreateStaff(req.Email, _hasher.Hash(req.Password), req.Role);

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Staff account created. NewUserId: {NewUserId}, Role: {Role}",
            user.Id, req.Role);

        return new StaffCreatedResponse(user.Id, user.Email.Value, user.Role);
    }
}