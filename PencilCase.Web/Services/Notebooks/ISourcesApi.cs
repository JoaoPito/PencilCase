using PencilCase.Shared.DTOs.Requests.Sources;
using PencilCase.Shared.DTOs.Responses.Parser;

namespace PencilCase.Web.Services.Notebooks;

public interface ISourcesApi
{
    public Task<Guid> StartUpload(UploadSourceRequest request);
    public Task<JobStatusResponse> GetUploadStatus(Guid jobId);
}