using Microsoft.AspNetCore.Mvc;

namespace AuthServer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigController(IConfiguration _configuration) : ControllerBase
{
    private const string TIMEOUT_CONFIG = "TimeOut";

    [HttpGet]

    public int GetConfig()
    {
        var value = _configuration.GetValue(TIMEOUT_CONFIG, -1);

        return value;
    }
}
