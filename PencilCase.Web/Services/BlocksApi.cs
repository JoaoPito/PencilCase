using System.Net.Http.Json;
using MudBlazor;
using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.Blocks;
using PencilCase.Web.Pages.Notebooks.Models;

namespace PencilCase.Web.Services;

public class BlocksApi : IBlocksApi
{
    private readonly HttpClient _httpClient;
    private readonly BlockMapper _blockMapper;

    public BlocksApi(IHttpClientFactory httpClientFactory, BlockMapper blockMapper)
    {
        _httpClient = httpClientFactory.CreateClient("BlocksAPI");
        _blockMapper = blockMapper;
    }
    
    public async Task<BlockViewModel?> GetBlock(Guid id)
    {
        var blockResponse = await _httpClient.GetFromJsonAsync<BlockResponse>($"{id}");
        if (blockResponse == null)
            throw new NullReferenceException("Response is null!");
        return _blockMapper.MapResponseToViewModel(blockResponse);
    }

    public async Task<BlockViewModel?> AddBlock(BlockViewModel block)
    {
        var request = _blockMapper.MapViewModelToPostRequest(block);
        var response = await _httpClient.PostAsJsonAsync<BlockPostRequest>($"", request!);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error while adding block with name '{block.Name}'");
        return await response.Content.ReadFromJsonAsync<BlockViewModel>();
    }

    public Task<BlockViewModel?> GetLastUsedBlock()
    {
        throw new NotImplementedException();
    }
    
    public async Task<IEnumerable<BlockViewModel>> LoadAllAsync(IEnumerable<Guid> blockIds)
    {
        var children = new List<BlockViewModel>();
        foreach (var id in blockIds)
        {
            var block = await GetBlock(id);
            if (block != null) children = children.Append<BlockViewModel>(block).ToList();
        }

        return children;
    }

    public async Task UpdateBlock(BlockViewModel block)
    {
        var request = _blockMapper.MapViewModelToPutRequest(block);
        var response = await _httpClient.PutAsJsonAsync<BlockPutRequest>($"{block.Id}", request!);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error while updating block with name '{block.Name}' and Id '{block.Id}'");
    }

    public async Task DeleteBlock(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{id}");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error while deleting block with Id '{id}'");
    }
}