using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : CustomBaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
    }

    [HttpPost("enable-two-factor-authentication")]
    public async Task<IActionResult> EnableTwoFactorAuthentication(string userId, string phoneNumber)
    {
        return ActionResultInstance(await _userService.EnableTwoFactorAuthentication(phoneNumber, userId));
    }

    [HttpPost("verify-two-factor")]
    public async Task<IActionResult> VerifyTwoFactor(string userId, string token, string phoneNumber)
    {
        return ActionResultInstance(await _userService.VerifyTwoFactorToken(phoneNumber, token, userId));
    }

    [HttpGet("verify-confirm-email")]
    public async Task<IActionResult> VerifyEmailConfirm(string userId, string confirmationToken)
    {
        return ActionResultInstance(await _userService.VerifyEmailConfirmTokenAsync(userId, confirmationToken));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        return ActionResultInstance(await _userService.GeneratePasswordResetTokenAsync(email));
    }

    [HttpPost("verify-reset-token")]
    public async Task<IActionResult> VerifyResetToken(string resetToken, string userId)
    {
        return ActionResultInstance(await _userService.VerifyPasswordResetTokenAsync(resetToken, userId));
    }

    [HttpPost("lock-user")]
    public async Task<IActionResult> LockUser(string userId)
    {
        return ActionResultInstance(await _userService.LockUser(userId));
    }
    [HttpPost("unlock-user")]
    public async Task<IActionResult> UnLockUser(string userId)
    {
        return ActionResultInstance(await _userService.UnlockUser(userId));
    }

    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword(string userId, string resetToken, string newPassword)
    {
        return ActionResultInstance(await _userService.UpdatePasswordAsync(resetToken, userId, newPassword));
    }

    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        return ActionResultInstance(await _userService.GetUserAsync(HttpContext.User.Claims.FirstOrDefault().Value));
    }
    //[Authorize]
    [HttpGet("create-user-roles")]
    public async Task<IActionResult> CreateUserRoles()
    {
        return ActionResultInstance(await _userService.CreateUserRole());
    }

    //[Authorize]
    [HttpPost("assign-rol-to-user")]
    public async Task<IActionResult> AssignRoleToUser(string userId, string roleId)
    {
        return ActionResultInstance(await _userService.AssignRoleToUser(userId, roleId));
    }

    [HttpPost("add-claim-to-user")]
    public async Task<IActionResult> AddClaimToUserAsync(string userId, string claimType, string claimValue)
    {
        return ActionResultInstance(await _userService.AddClaimToUserAsync(userId: userId, claimType: claimType, claimValue: claimValue));
    }
}
