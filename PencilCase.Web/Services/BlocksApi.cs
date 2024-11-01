using System.Net.Http.Json;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Web.Services;

public class BlocksApi : IBlocksApi
{
    private readonly HttpClient _httpClient;
    
    public BlocksApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BlocksAPI");
    }
    
    public async Task<Block?> GetBlock(string id)
    {
        var blockResponse = await _httpClient.GetFromJsonAsync<Block>($"{id}");
        if (blockResponse == null)
            throw new NullReferenceException("Response is null!");
        return blockResponse;
    }

    public void AddBlock(Block block)
    {
        throw new NotImplementedException();
    }

    Task<Block?> IBlocksApi.GetLastUsedBlock()
    {
        throw new NotImplementedException();
    }
}