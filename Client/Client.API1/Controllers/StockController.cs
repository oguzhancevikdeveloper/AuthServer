using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.API1.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StockController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStock()
    {

        var userEmailClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

        return Ok($"Stock işlemleri  =>UserName: {userEmailClaim}");
    }
}