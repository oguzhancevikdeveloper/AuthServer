using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using AuthServer.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Service.Services;

public class UserService : IUserService
{
    private readonly UserManager<UserApp> _userManager;
    private readonly RoleManager<IdentityRole>  _roleManager;

    public UserService(UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new UserApp { Email = createUserDto.Email,UserName = createUserDto.UserName,};

        var result = await _userManager.CreateAsync(user,createUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description).ToList();
            return Response<UserAppDto>.Fail(new ErrorDto(errors,true),400);
        }

        return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
    }

    public async Task<Response<NoDataDto>> CreateUserRole(string userId)
    {
        if (!await _roleManager.RoleExistsAsync("admin") )
        {
            await _roleManager.CreateAsync(new() { Name = "admin" });
            await _roleManager.CreateAsync(new() { Name = "manager" });
        }


        var user = await _userManager.FindByIdAsync(userId);

        await _userManager.AddToRoleAsync(user, "admin");
        await _userManager.AddToRoleAsync(user, "manager");

        return Response<NoDataDto>.Success(StatusCodes.Status201Created);
    }

    public async Task<Response<UserAppDto>> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return Response<UserAppDto>.Fail("Username not found", 404, true);

        return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
    }
}
