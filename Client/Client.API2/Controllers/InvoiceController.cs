﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.API2.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InvoiceController : ControllerBase
{
    [HttpGet]
    public IActionResult GetInvoices()
    {

        return Ok($"Secret client çalışıyor.");
    }
}

