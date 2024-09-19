﻿using Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Identity;

namespace Product.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController(ITokenService tokenService) : ControllerBase
{
    private readonly ITokenService _tokenService = tokenService;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        var result = _tokenService.GetToken(new TokenRequest());
        return Ok(result);
    }
}
