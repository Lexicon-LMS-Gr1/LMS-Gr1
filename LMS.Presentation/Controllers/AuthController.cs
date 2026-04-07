using LMS.Shared.DTOs.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IServiceManager serviceManager;

    public AuthController(IServiceManager serviceManager)
    {
        this.serviceManager = serviceManager;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Registrera en ny användare.",
        Description = "Skapar ett nytt användarkonto med de angivna registreringsdetaljerna."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Användaren har framgångsrikt registrerats.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Ogiltiga värden eller registreringen misslyckades.")]
    public async Task<IActionResult> RegisterUser(UserRegistrationDto userRegistrationDto)
    {
        IdentityResult result = await serviceManager.AuthService.RegisterUserAsync(userRegistrationDto);
        return result.Succeeded ? StatusCode(StatusCodes.Status201Created) : BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Autentisera användare.",
        Description = "Validerar användaruppgifter och returnerar en JWT-token för auktorisering."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Autentiseringen lyckades.", typeof(TokenDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Felaktigt användarnamn eller lösenord.")]
    public async Task<IActionResult> Authenticate(UserAuthDto user)
    {
        if (!await serviceManager.AuthService.ValidateUserAsync(user))
            return Unauthorized();

        var tokenDto = await serviceManager.AuthService.CreateTokenAsync(addTime: true);
        return Ok(tokenDto);
    }
}
