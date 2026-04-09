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

    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    [SwaggerOperation(
    Summary = "Begär återställning av lösenord.",
    Description = "Skickar en återställningslänk till användarens e‑post."
)]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto dto)
    {
        await serviceManager.AuthService.RequestPasswordResetAsync(dto.Email);

        return Ok(new { Message = "Om ett konto med den e‑postadressen finns har en länk skickats." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(
    Summary = "Återställ lösenord.",
    Description = "Återställer användarens lösenord med hjälp av en giltig token."
)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await serviceManager.AuthService.ResetPasswordAsync(dto);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Lösenordet har uppdaterats." });
    }

}
