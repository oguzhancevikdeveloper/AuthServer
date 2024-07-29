using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using AuthServer.Shared.Dtos;
using AuthServer.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Service.Services;

public class UserService(UserManager<UserApp> _userManager, RoleManager<IdentityRole> _roleManager, IMailService _mailService) : IUserService
{
    public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new UserApp { Email = createUserDto.Email, UserName = createUserDto.UserName, City = createUserDto.City };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description).ToList();
            return Response<UserAppDto>.Fail(new ErrorDto(errors, true), 400);
        }

        return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
    }

    public async Task<Response<NoDataDto>> CreateUserRole(string userId)
    {
        if (!await _roleManager.RoleExistsAsync("admin"))
        {
            await _roleManager.CreateAsync(new() { Name = "admin" });
            await _roleManager.CreateAsync(new() { Name = "manager" });
        }


        var user = await _userManager.FindByIdAsync(userId);

        await _userManager.AddToRoleAsync(user, "admin");
        await _userManager.AddToRoleAsync(user, "manager");

        return Response<NoDataDto>.Success(StatusCodes.Status201Created);
    }

    public async Task<Response<NoDataDto>> GeneratePasswordResetTokenAsync(string email)
    {
        UserApp? userApp = await _userManager.FindByEmailAsync(email);

        if (userApp == null) Response<NoDataDto>.Fail("User not a found", StatusCodes.Status404NotFound, true);

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(userApp);

        resetToken = resetToken.UrlEncode();

        await _mailService.SendPasswordResetMailAsync(userApp.Email, userApp.Id, resetToken);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);

    }

    public async Task<Response<UserAppDto>> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return Response<UserAppDto>.Fail("Username not found", 404, true);

        return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
    }

    public async Task<Response<NoDataDto>> UpdatePasswordAsync(string userId, string resetToken, string newPassword)
    {
        UserApp? userApp = await _userManager.FindByIdAsync(userId);

        if (userApp == null) return Response<NoDataDto>.Fail("User not a found",StatusCodes.Status400BadRequest,true);

        resetToken = resetToken.UrlDecode();

        IdentityResult identityResult = await _userManager.ResetPasswordAsync(userApp, resetToken, newPassword);

        if (!identityResult.Succeeded) return Response<NoDataDto>.Fail("Progress fail", StatusCodes.Status400BadRequest, true);

        await _userManager.UpdateSecurityStampAsync(userApp);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }

    public async Task<Response<NoDataDto>> VerifyPasswordResetTokenAsync(string resetToken, string userId)
    {
        UserApp? userApp = await _userManager.FindByIdAsync(userId);

        if (userApp == null) return Response<NoDataDto>.Fail("User not a found", StatusCodes.Status404NotFound, true);

        resetToken = resetToken.UrlDecode();

        await _userManager.VerifyUserTokenAsync(userApp, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", resetToken);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }
}
