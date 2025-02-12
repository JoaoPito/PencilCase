using System.Security.Claims;
using PencilCase.Shared.DTOs.Requests.Blocks;

namespace PencilCase.API.Handlers;

public interface IBlocksApiEndpointsHandler
{
    IResult GetById(Guid id, ClaimsPrincipal claims);
    IResult GetAllChildren(Guid id, ClaimsPrincipal claims);
    Task<IResult> CreateNew(BlockPostRequest request, ClaimsPrincipal claims);
    Task<IResult> Update(Guid id, BlockPutRequest request, ClaimsPrincipal claims);
    Task<IResult> Patch(Guid id, BlockPatchRequest request, ClaimsPrincipal claims);
    Task<IResult> Delete(Guid id, ClaimsPrincipal claims);
}