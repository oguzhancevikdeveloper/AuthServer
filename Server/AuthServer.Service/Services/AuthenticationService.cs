using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Shared.Dtos;
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

    public AuthenticationService(IOptions<List<Client>> optionsClients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenRepository)
    {
        _clients = optionsClients.Value;
        _tokenService = tokenService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _userRefreshTokenRepository = userRefreshTokenRepository;
    }

    public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
    {
        if (loginDto == null) { throw new ArgumentNullException(nameof(loginDto)); }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Response<TokenDto>.Fail("Email or Password incorrect", 400, true);

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password)) return Response<TokenDto>.Fail("Email or Password incorrect", 400, true);

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
