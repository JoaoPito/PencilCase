using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PencilCase.Shared.DTOs.Requests.Sources;
using PencilCase.Shared.DTOs.Responses.Parser;
using PencilCase.Shared.Models.LLM.Parser;
using PencilCase.Web.Client.Services.Sources;

namespace PencilCase.Web.Client.Pages.Upload;

public partial class SourceUploadDialog : ComponentBase
{
    [Parameter] public Guid ParentId { get; set; } = Guid.Empty;
    [Inject] private ISourcesApi SourceApi { get; set; } = null!;
    
    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
    
    private IBrowserFile? _file;
    private string _errorMsg = "";
    private bool _showError;
    
    private bool _processing;
    private string _processingMsg = "";
    
    private readonly TimeSpan jobWaitDelayDuration = TimeSpan.FromSeconds(5);
    
    private async Task UploadFileAsync()
    {
        _processing = true;
        StateHasChanged();

        if(_file == null)
            throw new ArgumentNullException($"Selected file is null");
        
        // Start upload job
        var request = await ConvertFileToUploadRequest(_file);
        var jobId = await SourceApi.StartUpload(request);

        JobStatusResponse jobStatus;
        while((jobStatus = await SourceApi.GetUploadStatus(jobId)).StatusCode != ParserJob.JobStatus.Completed)
        {
            _processingMsg = jobStatus.StatusMsg ?? "";
            StateHasChanged();
            await Task.Delay(jobWaitDelayDuration);
        }
            
        _processing = false;
        StateHasChanged();
    }
    
    private async Task<UploadSourceRequest> ConvertFileToUploadRequest(IBrowserFile file)
    {
        return new UploadSourceRequest()
        {
            ParentBlockId = ParentId,
            FileName = file.Name,
            FileContents = await ConvertFileContentsToBase64(file)
        };
    }
    
    private async Task<string> ConvertFileContentsToBase64(IBrowserFile file)
    {
        using var ms = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(ms);
        return Convert.ToBase64String(ms.ToArray());
    }
    
    private async Task ClearAsync()
    {
        await (_fileUpload?.ClearAsync() ?? Task.CompletedTask);
        _file = null;
        _showError = false;
        _errorMsg = "";
        ClearDragClass();
    }
}