using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.Service.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly List<Client> _clients;
    private readonly ITokenService _tokenService;
    private readonly UserManager<UserApp> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository;
    private readonly IGenericRepository<AspNetUserPhoneCode> _userPhoneRepository;
    private readonly ITwilioService _twilioService;

    public AuthenticationService(IOptions<List<Client>> optionsClients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenRepository, IGenericRepository<AspNetUserPhoneCode> userPhoneRepository, ITwilioService twilioService)
    {
        _clients = optionsClients.Value;
        _tokenService = tokenService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _userRefreshTokenRepository = userRefreshTokenRepository;
        _twilioService = twilioService;
        _userPhoneRepository = userPhoneRepository;
    }

    public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
    {
        if (loginDto == null) { throw new ArgumentNullException(nameof(loginDto)); }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (await _userManager.IsLockedOutAsync(user)) return Response<TokenDto>.Fail($"{user.Email} has been locked", 400, true);

        if (user == null) return Response<TokenDto>.Fail("Email or Password incorrect", 400, true);

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {

            await _userManager.AccessFailedAsync(user);
            return Response<TokenDto>.Fail("Email or Password incorrect", 400, true);
        }
        
        
        if (!user.TwoFactorEnabled)
        {
            var _token = _tokenService.CreateToken(user);
            var _userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserAppId.Equals(user.Id)).SingleOrDefaultAsync();

            if (_userRefreshToken == null) await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserAppId = user.Id,
                RefreshToken = _token.RefreshToken,
                ExpirationDate = _token.RefreshTokenExpirationDate
            });
            else
            {
                _userRefreshToken.RefreshToken = _token.RefreshToken;
                _userRefreshToken.ExpirationDate = _token.RefreshTokenExpirationDate;
            }

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(_token, 200);
        }
        var userPhoneValidate = await _userPhoneRepository.Where(x => x.PhoneLoginCode.Equals(loginDto.PhoneLoginCode) && x.UserApp.Id.Equals(user.Id)).FirstOrDefaultAsync();

        if (userPhoneValidate == null)
        {
            Random random = new Random();
            string phoneCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            await _userPhoneRepository.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                UserApp = user,
                PhoneLoginCode = phoneCode,
                CreatedDate = DateTime.Now,
            });

            await _unitOfWork.CommitAsync();

            //await _twilioService.SendSmsAsync(user.PhoneNumber, $"Your verification code is {phoneCode}");

            return Response<TokenDto>.Success(new TokenDto
            {
                AccessToken = null,
                AccessTokenExpirationDate = DateTime.Now,
                RefreshToken = null,
                RefreshTokenExpirationDate = DateTime.Now
            }, StatusCodes.Status200OK);

        }


        if ((DateTime.Now - userPhoneValidate.CreatedDate) > TimeSpan.FromMinutes(2))
        {

            _userPhoneRepository.Remove(userPhoneValidate);
            _unitOfWork.Commit();

            return Response<TokenDto>.Fail("Phone code has been expired", StatusCodes.Status404NotFound, true);
        }




        var token = _tokenService.CreateToken(user);
        var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserAppId.Equals(user.Id)).SingleOrDefaultAsync();

        if (userRefreshToken == null) await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
        {
            UserAppId = user.Id,
            RefreshToken = token.RefreshToken,
            ExpirationDate = token.RefreshTokenExpirationDate
        });
        else
        {
            userRefreshToken.RefreshToken = token.RefreshToken;
            userRefreshToken.ExpirationDate = token.RefreshTokenExpirationDate;
        }

        _userPhoneRepository.Remove(userPhoneValidate);
        await _unitOfWork.CommitAsync();

        return Response<TokenDto>.Success(token, 200);

    }

    public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
    {

        var client = _clients.SingleOrDefault(x => x.Id.Equals(clientLoginDto.ClientId) && x.Secret.Equals(clientLoginDto.ClientSecret));

        if (client == null) return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", 404, true);

        var token = _tokenService.CreateTokenByClient(client);

        return Response<ClientTokenDto>.Success(token, 200);
    }

    public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
    {
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.RefreshToken.Equals(refreshToken)).SingleOrDefaultAsync();

        if (existRefreshToken == null) return Response<TokenDto>.Fail("RefreshToken not found", 404, true);

        var user = await _userManager.FindByIdAsync(existRefreshToken.UserAppId);
        if (user == null) return Response<TokenDto>.Fail("User not found", 404, true);

        var tokenDto = _tokenService.CreateToken(user);

        existRefreshToken.RefreshToken = tokenDto.RefreshToken;
        existRefreshToken.ExpirationDate = tokenDto.RefreshTokenExpirationDate;

        await _unitOfWork.CommitAsync();

        return Response<TokenDto>.Success(tokenDto, 200);
    }

    public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
    {
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.RefreshToken.Equals(refreshToken)).SingleOrDefaultAsync();

        if (existRefreshToken == null) return Response<NoDataDto>.Fail("RefreshToken not found", 404, true);

        _userRefreshTokenRepository.Remove(existRefreshToken);

        await _unitOfWork.CommitAsync();

        return Response<NoDataDto>.Success(200);
    }
}
