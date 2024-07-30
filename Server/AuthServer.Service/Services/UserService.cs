using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Shared.Dtos;
using AuthServer.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Service.Services;

public class UserService : IUserService
{
    private readonly ITwilioService _twilioService;
    private readonly UserManager<UserApp> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMailService _mailService;
    private readonly IGenericRepository<AspNetUserPhoneCode> _userPhoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(ITwilioService twilioService, UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager, IMailService mailService, IGenericRepository<AspNetUserPhoneCode> userPhoneRepository, IUnitOfWork unitOfWork)
    {
        _twilioService = twilioService;
        _userManager = userManager;
        _roleManager = roleManager;
        _mailService = mailService;
        _userPhoneRepository = userPhoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Response<NoDataDto>> AssignRoleToUser(string userId, string roleId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var role = await _roleManager.FindByIdAsync(roleId);

        if (user != null && role != null) await _userManager.AddToRoleAsync(user: user, role.Name);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }

    public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new UserApp { Email = createUserDto.Email, UserName = createUserDto.UserName, City = createUserDto.City, PhoneNumber = createUserDto.PhoneNumber };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description).ToList();
            return Response<UserAppDto>.Fail(new ErrorDto(errors, true), 400);
        }

        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        confirmToken = confirmToken.UrlEncode();
        await _mailService.SendEmailConfirmationLinkAsync(to: user.Email, user.Id, confirmToken);


        return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
    }

    public async Task<Response<NoDataDto>> CreateUserRole()
    {
        if (!await _roleManager.RoleExistsAsync("admin"))
        {
            var roles = new List<string> { "admin", "manager", "user" };
            foreach (var role in roles)
            {
                var identityRole = new IdentityRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                await _roleManager.CreateAsync(identityRole);
            }
        }
        return Response<NoDataDto>.Success(StatusCodes.Status201Created);
    }

    public async Task<Response<NoDataDto>> EnableTwoFactorAuthentication(string phoneNumber, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Response<NoDataDto>.Fail("User not a Found", StatusCodes.Status404NotFound, true);


        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

        await _userPhoneRepository.AddAsync(new()
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.Now,
            PhoneLoginCode = code,
            UserApp = user
        });

        await _unitOfWork.CommitAsync();

        //await _twilioService.SendSmsAsync(phoneNumber, $"Your verification code is {token}");
        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
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

        if (userApp == null) return Response<NoDataDto>.Fail("User not a found", StatusCodes.Status400BadRequest, true);

        resetToken = resetToken.UrlDecode();

        IdentityResult identityResult = await _userManager.ResetPasswordAsync(userApp, resetToken, newPassword);

        if (!identityResult.Succeeded) return Response<NoDataDto>.Fail("Progress fail", StatusCodes.Status400BadRequest, true);

        await _userManager.UpdateSecurityStampAsync(userApp);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }

    public async Task<Response<NoDataDto>> VerifyEmailConfirmTokenAsync(string userId, string confirmationToken)
    {
        UserApp? user = await _userManager.FindByIdAsync(userId);

        if (user == null) return Response<NoDataDto>.Fail("User not a found", StatusCodes.Status400BadRequest, true);

        confirmationToken = confirmationToken.UrlDecode();

        var resultVerify = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.EmailConfirmationTokenProvider, "EmailConfirmation", confirmationToken);

        var resultConfirm = await _userManager.ConfirmEmailAsync(user, confirmationToken);
        if (!resultConfirm.Succeeded) return Response<NoDataDto>.Fail("Bad request", StatusCodes.Status400BadRequest, true);

        await _userManager.UpdateSecurityStampAsync(user);

        if (!resultVerify) return Response<NoDataDto>.Fail("User not a found", StatusCodes.Status400BadRequest, true);

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

    public async Task<Response<NoDataDto>> VerifyTwoFactorToken(string phoneNumber, string token, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return Response<NoDataDto>.Fail("User not a found", StatusCodes.Status200OK, true);

        var userPhone = _userPhoneRepository.Where(x => x.UserApp.Equals(user) && x.PhoneLoginCode.Equals(token)).FirstOrDefaultAsync().Result;

        if ((DateTime.Now - userPhone.CreatedDate) < TimeSpan.FromHours(1))
        {
            user.TwoFactorEnabled = true;
            user.PhoneNumberConfirmed = true;
            user.PhoneNumber = phoneNumber;
            await _userManager.UpdateAsync(user);

            _userPhoneRepository.Remove(userPhone);
            _unitOfWork.Commit();
            return Response<NoDataDto>.Success(StatusCodes.Status200OK);
        }

        return Response<NoDataDto>.Fail("Bad Request", StatusCodes.Status400BadRequest, true);


    }
}
