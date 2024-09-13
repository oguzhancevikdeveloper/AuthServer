using AuthServer.Core.DTOs;
using AuthServer.Shared.Dtos;

namespace AuthServer.Core.Services;

public interface IAuthenticationService
{
    Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto);
    Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken);
    Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken);
    Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto);

    Task<Response<TokenDto>> GoogleLoginAsync(string idToken, int accessToken);
    Task<Response<TokenDto>> FacebookLoginAsync(string idToken, int accessToken);

}
