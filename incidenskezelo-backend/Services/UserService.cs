using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Services;

public enum UserActionStatus
{
    Success,
    NotFound,
    Invalid,
    Conflict,
}

public record UserActionResult(UserActionStatus Status, UserResponse? User, string? Error);

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    PendingApproval,
    Deactivated,
}

public record LoginResult(LoginStatus Status, LoginResponse? Response);

public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly TokenService _tokenService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, TokenService tokenService, TimeProvider timeProvider, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
        _logger = logger;
    }


    public async Task<UserActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await CreateUserAsync(request, UserRole.Pending, cancellationToken);

        if (result.Status == UserActionStatus.Success)
        {
            _logger.LogInformation("User {UserId} registered and is waiting for approval", result.User!.UserId);
        }

        return result;
    }


    public async Task<bool> NeedsSetupAsync(CancellationToken cancellationToken)
    {
        return !await _dbContext.Users.AnyAsync(user => user.Role == UserRole.Admin && user.IsActive, cancellationToken);
    }


    public async Task<UserActionResult> CreateFirstAdminAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!await NeedsSetupAsync(cancellationToken))
        {
            return new UserActionResult(UserActionStatus.Conflict, null, "Setup is already complete.");
        }

        var result = await CreateUserAsync(request, UserRole.Admin, cancellationToken);

        if (result.Status == UserActionStatus.Success)
        {
            _logger.LogInformation("First admin account {UserId} created through setup", result.User!.UserId);
        }

        return result;
    }


    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResult(LoginStatus.InvalidCredentials, null);
        }

        if (!user.IsActive)
        {
            return new LoginResult(LoginStatus.Deactivated, null);
        }

        if (user.Role == UserRole.Pending)
        {
            return new LoginResult(LoginStatus.PendingApproval, null);
        }

        var (token, expiresAtUtc) = _tokenService.CreateToken(user);

        return new LoginResult(LoginStatus.Success, new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = UserResponse.From(user),
        });
    }


    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .OrderBy(user => user.Role == UserRole.Pending ? 0 : 1)
            .ThenBy(user => user.Name)
            .ToListAsync(cancellationToken);

        return users.Select(UserResponse.From).ToList();
    }


    public async Task<List<AssignableUserResponse>> GetAssignableAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Where(user => user.IsActive && user.Role != UserRole.Pending)
            .OrderBy(user => user.Name)
            .Select(user => new AssignableUserResponse { UserId = user.UserId, Name = user.Name })
            .ToListAsync(cancellationToken);
    }


    public async Task<UserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);

        return user is null ? null : UserResponse.From(user);
    }


    public async Task<UserActionResult> ChangeRoleAsync(int actorUserId, int userId, string roleName, CancellationToken cancellationToken)
    {
        if (!TryParseName(roleName, out UserRole newRole))
        {
            return Invalid($"Unknown role '{roleName}'.");
        }

        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return new UserActionResult(UserActionStatus.NotFound, null, null);
        }

        if (user.UserId == actorUserId)
        {
            return Invalid("You cannot change your own role.");
        }

        if (user.Role == UserRole.Admin && newRole != UserRole.Admin && await IsLastActiveAdminAsync(user, cancellationToken))
        {
            return Invalid("The last active admin cannot lose the admin role.");
        }

        user.Role = newRole;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {ActorUserId} changed the role of user {UserId} to {Role}", actorUserId, userId, newRole);

        return Success(user);
    }


    public async Task<UserActionResult> SetActiveAsync(int actorUserId, int userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null)
        {
            return new UserActionResult(UserActionStatus.NotFound, null, null);
        }

        if (user.UserId == actorUserId)
        {
            return Invalid("You cannot deactivate your own account.");
        }

        if (!isActive && user.Role == UserRole.Admin && await IsLastActiveAdminAsync(user, cancellationToken))
        {
            return Invalid("The last active admin cannot be deactivated.");
        }

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {ActorUserId} set user {UserId} active = {IsActive}", actorUserId, userId, isActive);

        return Success(user);
    }


    private async Task<UserActionResult> CreateUserAsync(RegisterRequest request, UserRole role, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var name = request.Name.Trim();

        if (name.Length == 0)
        {
            return Invalid("Name is required.");
        }

        if (await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return new UserActionResult(UserActionStatus.Conflict, null, "An account with this email already exists.");
        }

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success(user);
    }


    private async Task<bool> IsLastActiveAdminAsync(User admin, CancellationToken cancellationToken)
    {
        return !await _dbContext.Users.AnyAsync(
            user => user.UserId != admin.UserId && user.Role == UserRole.Admin && user.IsActive,
            cancellationToken);
    }


    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }


    private static bool TryParseName<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: true, out result)
            && Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);
    }


    private static UserActionResult Success(User user)
    {
        return new UserActionResult(UserActionStatus.Success, UserResponse.From(user), null);
    }


    private static UserActionResult Invalid(string error)
    {
        return new UserActionResult(UserActionStatus.Invalid, null, error);
    }
}
