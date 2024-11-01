using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Web.Services;

public class BlocksApi : IBlocksApi
{
    private readonly HttpClient _httpClient;
    
    public BlocksApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BlocksAPI");
    }
    
    public Block GetBlock(string id)
    {
        throw new NotImplementedException();
    }

    public Block GetLastUsedBlock()
    {
        throw new NotImplementedException();
    }
}