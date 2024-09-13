using AuthServer.Core.DTOs;
using AuthServer.Shared.Dtos;

namespace AuthServer.Core.Services;

public interface IUserService
{
    Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<Response<NoDataDto>> EnableTwoFactorAuthentication(string phoneNumber, string userId);
    Task<Response<NoDataDto>> VerifyTwoFactorToken(string phoneNumber, string token, string userId);
    Task<Response<UserAppDto>> GetUserAsync(string userId);
    Task<Response<NoDataDto>> CreateUserRole();
    Task<Response<NoDataDto>> AssignRoleToUser(string userId, string roleId);
    Task<Response<NoDataDto>> VerifyEmailConfirmTokenAsync(string userId, string confirmationToken);
    Task<Response<NoDataDto>> GeneratePasswordResetTokenAsync(string email);
    Task<Response<NoDataDto>> VerifyPasswordResetTokenAsync(string resetToken, string userId);
    Task<Response<NoDataDto>> UpdatePasswordAsync(string userId, string resetToken, string newPassword);
    Task<Response<NoDataDto>> LockUser(string userId);
    Task<Response<NoDataDto>> UnlockUser(string userId);
    Task<Response<NoDataDto>> AddClaimToUserAsync(string userId, string claimType, string claimValue);
    Task<Response<NoDataDto>> AddRoleWithClaimsToUser(string userId, string roleId, string claimType, List<string> claimValue);


}
