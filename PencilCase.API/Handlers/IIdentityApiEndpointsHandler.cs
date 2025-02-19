using PencilCase.Shared.DTOs.Requests.Identity;

namespace PencilCase.API.Handlers;

public interface IIdentityApiEndpointsHandler
{
    public Task<IResult> RegisterUserAsync(RegisterUserRequestDto request);
    public Task<IResult> LoginUserAsync(LoginUserRequestDto request, bool? useCookies, bool? useSessionCookies);
    public Task<IResult> RefreshTokenAsync(RefreshTokenRequestDto request);
    public Task<IResult> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    public Task<IResult> ResetPasswordAsync(ResetPasswordRequestDto request);
}