using PencilCase.Shared.Models.Download;

namespace PencilCase.Web.Services;

public interface IFileDownloader
{
    public Task DownloadFile(FileDownload file);
}