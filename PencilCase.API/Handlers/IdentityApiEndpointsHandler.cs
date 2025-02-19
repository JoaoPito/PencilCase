using Microsoft.AspNetCore.Identity;
using PencilCase.Identity.Models;
using PencilCase.Shared.DTOs.Requests.Identity;

namespace PencilCase.API.Handlers;

public class IdentityApiEndpointsHandler(UserManager<AppUser> userManager): IIdentityApiEndpointsHandler
{
    public async Task<IResult> RegisterUserAsync(RegisterUserRequestDto request)
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException($"{nameof(RegisterUserAsync)} requires a user store with email support.");
        }

        var email = request.Email;

        if (string.IsNullOrEmpty(email)) // Validate email
        {
            return Results.BadRequest();
        }

        var user = new AppUser();
        // COnfigure user attributes
        
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Results.Problem();
        }

        return Results.Ok();
    }

    public Task<IResult> LoginUserAsync(LoginUserRequestDto request, bool? useCookies, bool? useSessionCookies)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        throw new NotImplementedException();
    }
}