using System.Text.RegularExpressions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using PencilCase.LLM.Parser.Services;
using PencilCase.Shared.DTOs.Requests.Sources;
using PencilCase.Shared.Models.LLM.Parser;

namespace PencilCase.API.Endpoints;

public static class SourcesApiExtensions
{
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".txt", ".md"];
    private static readonly HashSet<char> InvalidPathChars = new(
        Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars())
    );

    private static readonly string SourceUploadConfig = "LlmApi:UploadSettings:MaxFileSize";
    
    public static void AddV1SourceEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var sourcesGroup = app.MapGroup("/api/v{version:apiVersion}/sources")
            .WithApiVersionSet(apiVersionSet)
            .WithTags(["Sources", "RAG"]);
        
        sourcesGroup.MapPost("", async (
            [FromServices] IConfiguration configuration,
            [FromServices] IParserProducerService brokerService,
            [FromBody] UploadSourceRequest request) =>
        {
            // Validate and Sanitize File
            var maxFileSize = configuration.GetValue<uint?>(SourceUploadConfig) ?? 1024 * 1024;
            ValidateFile(request, maxFileSize);
            
            // Create Job
            var job = new ParserJob
            {
                Status = ParserJob.JobStatus.Accepted,
                ParentBlockId = request.ParentBlockId,
                File = new()
                {
                    Name = SanitizeFileName(request.FileName),
                    Contents = request.FileContents,
                }
            };
            
            // Submit Job
            await brokerService.SubmitJobAsync(job);

            // Respond with Job ID and link
            return Results.AcceptedAtRoute("GetSourceStatus", new { jobId = job.Id });
        })
        .WithName("UploadSource")
        .WithDescription("Starts a parsing job, it parses a file into Markdown, adds it to the VectorDB, and creates a new block and adds it to the Blocks DB");

        sourcesGroup.MapGet("{jobId}", async (
            Guid jobId,
            [FromServices] IParserProducerService brokerService) =>
        {
            try
            {
                var job = await brokerService.GetJobAsync(jobId);
                return Results.Ok(MapJobToJobStatusResponse(job));
            }
            catch (ArgumentException e)
            {
                return Results.NotFound(e.Message);
            }
        })
        .WithName("GetSourceStatus")
        .WithDescription("Get status of a running parsing job.");
    }

    private static object? MapJobToJobStatusResponse(ParserJob job)
    {
        return new JobStatusResponse
        {
            StatusCode = job.Status,
            StatusMsg = job.StatusMsg,
        };
    }

    static void ValidateFile(UploadSourceRequest request, uint maxFileSize)
    {
        if(string.IsNullOrEmpty(request.FileName))
            throw new ArgumentException("File name is required");
        
        if(!ValidateExtension(request.FileName))
            throw new ArgumentException("File extension is not supported");

        var contentBytes = Convert.FromBase64String(request.FileContents);
        
        if(contentBytes.Length > maxFileSize)
            throw new ArgumentException($"File size exceeds {maxFileSize / 1024 / 1024}MB");
    }

    static bool ValidateExtension(string filename)
    {
        string extension = Path.GetExtension(filename);
        return AllowedExtensions.Contains(extension);
    }

    static string SanitizeFileName(string filename)
    {
        var extension = Path.GetExtension(filename);
        var name = Path.GetFileNameWithoutExtension(filename);
        string sanitizedName = new string(name
            .Select(ch => InvalidPathChars.Contains(ch) ? '_' : ch)
            .ToArray());

        sanitizedName = Regex.Replace(
            sanitizedName, 
            @"[\s_]+", "_")
            .Trim('_');

        return sanitizedName + extension;
    }
}