using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Share.Models.Auth;

namespace ApiService.Endpoints;

public sealed class ExternalAuthEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ExternalAuth").WithTags("ExternalAuth");
        group.MapGet("/signin-microsoft", SignInMicrosoft);
        group.MapGet("/signin-google", SignInGoogle);
        group.MapGet("/getToken", GetToken);
    }

    private static IResult SignInMicrosoft(HttpContext context, string returnUrl = "/") =>
        Challenge(context, MicrosoftAccountDefaults.AuthenticationScheme, returnUrl);

    private static IResult SignInGoogle(HttpContext context, string returnUrl = "/") =>
        Challenge(context, GoogleDefaults.AuthenticationScheme, returnUrl);

    private static IResult Challenge(HttpContext context, string scheme, string returnUrl)
    {
        var redirectUri = $"/api/ExternalAuth/getToken?type={Uri.EscapeDataString(scheme)}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return TypedResults.Challenge(
            new AuthenticationProperties { RedirectUri = redirectUri },
            [scheme]);
    }

    private static async Task<IResult> GetToken(
        HttpContext context,
        ILogger<ExternalAuthEndpoints> logger,
        string type,
        string returnUrl = "/")
    {
        logger.LogInformation("{type} login callback initiated.", type);
        var result = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("External authentication failed for type: {type}", type);
            return TypedResults.BadRequest("External authentication failed.");
        }

        var externalUser = result.Principal;
        return TypedResults.Ok(new ExternalUserInfoDto
        {
            Email = externalUser.FindFirst(ClaimTypes.Email)?.Value,
            Name = externalUser.FindFirst(ClaimTypes.Name)?.Value,
        });
    }
}
