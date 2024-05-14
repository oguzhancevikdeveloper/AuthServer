﻿using AuthServer.Core.DTOs;
using AuthServer.Shared.Dtos;

namespace AuthServer.Core.Services;

public interface IUserService 
{
    Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<Response<UserAppDto>> GetUserAsync(string userId);
    Task<Response<NoDataDto>> CreateUserRole(string userId);
}
