using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PencilCase.Shared.DTOs.Requests.Sources;

namespace PencilCase.Web.Pages.Notebooks.Upload;

public partial class SourceUploadDialog : ComponentBase
{
    [Parameter] public Guid ParentId { get; set; } = Guid.Empty;
    
    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
    
    private IBrowserFile? _file;
    private string _errorMsg = "";
    private bool _showError;
    private bool _processing;
    
    private async Task UploadFileAsync()
    {
        _processing = true;
        StateHasChanged();
        
        await Task.Delay(5000);
        
        // Start upload job
        // While job is not completed
            // At each N seconds, get the job status and show to the user
            
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