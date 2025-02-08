using Asp.Versioning;
using Asp.Versioning.Builder;
using PencilCase.Identity.Models;

namespace PencilCase.API.Endpoints;

public static class IdentityApiExtensions
{
    public static void AddV1IdentityApiEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("Users");

        group.MapIdentityApi<AppUser>();
    }
}