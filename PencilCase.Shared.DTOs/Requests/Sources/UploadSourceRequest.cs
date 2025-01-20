namespace PencilCase.Shared.DTOs.Requests.Sources;

public class UploadSourceRequest
{
    public string FileName { get; set; }
    public string FileContents { get; set; }
}