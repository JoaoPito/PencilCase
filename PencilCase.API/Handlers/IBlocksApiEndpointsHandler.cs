using System.Security.Claims;
using PencilCase.Shared.DTOs.Requests.Blocks;

namespace PencilCase.API.Handlers;

public interface IBlocksApiEndpointsHandler
{
    IResult GetById(Guid id, ClaimsPrincipal claims);
    IResult GetAllChildren(Guid id, ClaimsPrincipal claims);
    Task<IResult> CreateNewAsync(BlockPostRequest request, ClaimsPrincipal claims);
    Task<IResult> UpdateAsync(Guid id, BlockPutRequest request, ClaimsPrincipal claims);
    Task<IResult> PatchAsync(Guid id, BlockPatchRequest request, ClaimsPrincipal claims);
    Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal claims);
}