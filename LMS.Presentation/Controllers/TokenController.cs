using LMS.Shared.DTOs.AuthDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/token")]
[ApiController]
public class TokenController(IAuthService authenticationService) : ControllerBase
{
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Uppdatera JWT-token.",
        Description = "Tar en existerande access token och refresh token, validerar dem, och utfärdar ett nytt token-par."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Token framgångsrikt uppdaterad.", typeof(TokenDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Ogiltig token-data eller refresh token utgången.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token kunde inte uppdateras p.g.a. autentiseringsfel.")]
    public async Task<ActionResult<TokenDto>> RefreshToken(TokenDto token) =>
         Ok(await authenticationService.RefreshTokenAsync(token));
}
