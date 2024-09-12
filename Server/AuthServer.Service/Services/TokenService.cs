using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using AuthServer.Shared.Configuration;
using AuthServer.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthServer.Service.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<UserApp> _userManager;
    private readonly CustomTokenOption _customTokenOption;

    public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOption> options)
    {
        _userManager = userManager;
        _customTokenOption = options.Value;
    }

    public TokenDto CreateToken(UserApp userApp)
    {
        var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);
        var refreshTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.RefreshTokenExpiration);
        var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOption.SecurityKey);
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _customTokenOption.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: GetClaims(userApp, _customTokenOption.Audience).Result,
            signingCredentials: signingCredentials
            );

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(jwtSecurityToken);
        var tokenDto = new TokenDto
        {
            AccessToken = token,
            AccessTokenExpirationDate = accessTokenExpiration,
            RefreshToken = CreateRefreshToken(),
            RefreshTokenExpirationDate = refreshTokenExpiration,
        };

        return tokenDto;
    }

    public ClientTokenDto CreateTokenByClient(Client client)
    {
        var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);

        var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOption.SecurityKey);
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new(

            issuer: _customTokenOption.Issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: GetClaimsByClient(client),
            signingCredentials: signingCredentials
            );

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(jwtSecurityToken);
        var clientTokenDto = new ClientTokenDto
        {
            AccessToken = token,
            AccessTokenExpirationDate = accessTokenExpiration,
        };

        return clientTokenDto;
    }

    private string CreateRefreshToken()
    {
        var numberByte = new Byte[32];
        using var rnd = RandomNumberGenerator.Create();
        rnd.GetBytes(numberByte);
        return Convert.ToBase64String(numberByte);
    }

    private async Task<IEnumerable<Claim>> GetClaims(UserApp userApp, List<string> audience)
    {
        var userRoles = await _userManager.GetRolesAsync(userApp);
        var userRoleClaims = await _userManager.GetClaimsAsync(userApp);
        var userList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,userApp.Id),
            new Claim(JwtRegisteredClaimNames.Email,userApp.Email),
            new Claim(ClaimTypes.Role,userApp.UserName),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new Claim("city",userApp.City),
            new Claim("birth-date",userApp.BirthDate.ToShortDateString())
        };


        userList.AddRange(audience.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
        userList.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));
        userList.AddRange(userRoleClaims.Select(c => new Claim(c.Type,c.Value)));
        return userList;

    }

    private IEnumerable<Claim> GetClaimsByClient(Client client)
    {
        var claims = new List<Claim>();
        claims.AddRange(client.Auidiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString());

        return claims;

    }
}
