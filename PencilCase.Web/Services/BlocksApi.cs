using System.Net.Http.Json;
using MudBlazor;
using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.Blocks;

namespace PencilCase.Web.Services;

public class BlocksApi : IBlocksApi
{
    private readonly HttpClient _httpClient;
    
    public BlocksApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BlocksAPI");
    }
    
    public async Task<BlockResponse?> GetBlock(Guid id)
    {
        var blockResponse = await _httpClient.GetFromJsonAsync<BlockResponse>($"{id}");
        if (blockResponse == null)
            throw new NullReferenceException("Response is null!");
        return blockResponse;
    }

    public async Task AddBlock(BlockPostRequest block)
    {
        var response = await _httpClient.PostAsJsonAsync<BlockPostRequest>($"", block);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error while adding block with name '{block.Name}'");
    }

    public async Task<BlockResponse?> GetLastUsedBlock()
    {
        throw new NotImplementedException();
    }
    
    public async Task<IEnumerable<BlockResponse>> LoadAllAsync(IEnumerable<Guid> childrenIds)
    {
        var children = new List<BlockResponse>();
        foreach (var id in childrenIds)
        {
            var child = await GetBlock(id);
            if (child != null) children = children.Append<BlockResponse>(child).ToList();
        }

        return children;
    }
}