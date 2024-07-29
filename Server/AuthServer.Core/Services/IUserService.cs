using AuthServer.Core.DTOs;
using AuthServer.Shared.Dtos;

namespace AuthServer.Core.Services;

public interface IUserService 
{
    Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<Response<UserAppDto>> GetUserAsync(string userId);
    Task<Response<NoDataDto>> CreateUserRole(string userId);


    Task<Response<NoDataDto>> GeneratePasswordResetTokenAsync(string email);
    Task<Response<NoDataDto>> VerifyPasswordResetTokenAsync(string resetToken, string userId);
    Task<Response<NoDataDto>> UpdatePasswordAsync(string userId, string resetToken, string newPassword);

}
