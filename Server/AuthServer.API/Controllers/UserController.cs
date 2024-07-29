using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword(string userId, string resetToken, string newPassword)
    {
        return ActionResultInstance(await _userService.UpdatePasswordAsync(resetToken, userId, newPassword));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        return ActionResultInstance(await _userService.GetUserAsync(HttpContext.User.Claims.FirstOrDefault().Value));
    }
    [Authorize]
    [HttpPost("CreateUserRoles")]
    public async Task<IActionResult> CreateUserRoles(string userId)
    {
        return ActionResultInstance(await _userService.CreateUserRole(userId));
    }
}
