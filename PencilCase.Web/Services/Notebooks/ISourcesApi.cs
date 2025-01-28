using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.Web.Services.Notebooks;

public interface ISourcesApi
{
    public Task StartUpload(ParserFile file);
    public Task GetUploadStatus(Guid jobId);
}