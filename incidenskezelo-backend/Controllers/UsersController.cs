using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Contracts;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace incidenskezelo_backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);

        return result.Status == UserActionStatus.Success
            ? CreatedAtAction(nameof(GetById), new { userId = result.User!.UserId }, result.User)
            : ToErrorResult(result);
    }


    [HttpGet("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSetupStatus(CancellationToken cancellationToken)
    {
        return Ok(new { needsSetup = await _userService.NeedsSetupAsync(cancellationToken) });
    }


    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFirstAdmin([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateFirstAdminAsync(request, cancellationToken);

        return result.Status == UserActionStatus.Success
            ? CreatedAtAction(nameof(GetById), new { userId = result.User!.UserId }, result.User)
            : ToErrorResult(result);
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.LoginAsync(request, cancellationToken);

        return result.Status switch
        {
            LoginStatus.Success => Ok(result.Response),
            LoginStatus.PendingApproval => Problem("Your account is waiting for admin approval.", statusCode: StatusCodes.Status403Forbidden),
            LoginStatus.Deactivated => Problem("Your account has been deactivated.", statusCode: StatusCodes.Status403Forbidden),
            _ => Problem("Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized),
        };
    }


    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(CurrentUserId(), cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }


    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }


    [HttpGet("{userId:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> GetById(int userId, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(userId, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }


    [HttpPut("{userId:int}/role")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> ChangeRole(int userId, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.ChangeRoleAsync(CurrentUserId(), userId, request.Role, cancellationToken);

        return result.Status == UserActionStatus.Success ? Ok(result.User) : ToErrorResult(result);
    }


    [HttpPut("{userId:int}/active")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> SetActive(int userId, [FromBody] UpdateActiveRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.SetActiveAsync(CurrentUserId(), userId, request.IsActive!.Value, cancellationToken);

        return result.Status == UserActionStatus.Success ? Ok(result.User) : ToErrorResult(result);
    }


    [HttpGet("roles")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public IActionResult GetRoles()
    {
        return Ok(Enum.GetNames<UserRole>());
    }


    private int CurrentUserId()
    {
        return int.Parse(User.FindFirst(TokenClaims.UserId)!.Value);
    }


    private IActionResult ToErrorResult(UserActionResult result)
    {
        return result.Status switch
        {
            UserActionStatus.NotFound => NotFound(),
            UserActionStatus.Conflict => Problem(result.Error, statusCode: StatusCodes.Status409Conflict),
            _ => Problem(result.Error, statusCode: StatusCodes.Status400BadRequest),
        };
    }
}
