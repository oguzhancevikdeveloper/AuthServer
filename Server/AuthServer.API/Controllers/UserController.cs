﻿using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : CustomBaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var t = HttpContext.User.Identity.Name;
        return ActionResultInstance(await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name));
    }
}