using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

namespace HotChocolate.AspNetCore;

/// <summary>
/// Enables serving static files for a given request path
/// </summary>
public sealed class ToolStaticFileMiddleware
{
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly IFileProvider _fileProvider;
    private readonly PathString _matchUrl;
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates a new instance of the ToolStaticFileMiddleware.
    /// </summary>
    public ToolStaticFileMiddleware(
        RequestDelegate next,
        IFileProvider fileProvider,
        PathString matchUrl)
    {
        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        if (fileProvider is null)
        {
            throw new ArgumentNullException(nameof(fileProvider));
        }

        _next = next;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
        _fileProvider = fileProvider;
        _matchUrl = matchUrl;
    }

    /// <summary>
    /// Processes a request to determine if it matches a known file, and if so, serves it.
    /// </summary>
    public Task Invoke(HttpContext context)
    {
        if (context.Request.IsGetOrHeadMethod() &&
            context.Request.TryMatchPath(_matchUrl, false, out var subPath) &&
            _contentTypeProvider.TryGetContentType(subPath.Value!, out var contentType) &&
            (context.GetGraphQLToolOptions()?.Enable ?? true))
        {
            return TryServeStaticFile(context, contentType, subPath);
        }

        return _next(context);
    }

    private Task TryServeStaticFile(HttpContext context, string contentType, PathString subPath)
    {
        if (LookupFileInfo(subPath, contentType, out var fileInfo))
        {
            return SendAsync(context, fileInfo);
        }

        return _next(context);
    }

    private bool LookupFileInfo(
        PathString subPath,
        string contentType,
        out StaticFileInfo staticFileInfo)
    {
        var fileInfo = _fileProvider.GetFileInfo(subPath);

        if (fileInfo.Exists)
        {
            var length = fileInfo.Length;

            var last = fileInfo.LastModified;

            // Truncate to the second.
            var lastModified = new DateTimeOffset(
                last.Year, last.Month, last.Day, last.Hour,
                last.Minute, last.Second, last.Offset)
                .ToUniversalTime();

            var etagHash = lastModified.ToFileTime() ^ length;
            var etag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');

            staticFileInfo = new StaticFileInfo(fileInfo, etag, contentType);
            return true;
        }

        staticFileInfo = default;
        return false;
    }

    private static async Task SendAsync(HttpContext context, StaticFileInfo fileInfo)
    {
        SetCompressionMode(context);
        context.Response.StatusCode = 200;
        context.Response.ContentLength = fileInfo.File.Length;
        context.Response.ContentType = fileInfo.ContentType;

        var headers = context.Response.GetTypedHeaders();
        headers.LastModified = fileInfo.File.LastModified;
        headers.ETag = fileInfo.EntityTagHeader;
        headers.Headers[HeaderNames.AcceptRanges] = "bytes";

        try
        {
            await context.Response.SendFileAsync(
                fileInfo.File,
                0,
                fileInfo.File.Length,
                context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            context.Abort();
        }
    }

    private static void SetCompressionMode(HttpContext context)
    {
        if (context.Features.Get<IHttpsCompressionFeature>() is { } c)
        {
            c.Mode = HttpsCompressionMode.Default;
        }
    }

    private readonly struct StaticFileInfo
    {
        public StaticFileInfo(
            IFileInfo file,
            EntityTagHeaderValue entityTagHeader,
            string contentType)
        {
            File = file;
            EntityTagHeader = entityTagHeader;
            ContentType = contentType;
        }

        public IFileInfo File { get; }

        public EntityTagHeaderValue EntityTagHeader { get; }

        public string ContentType { get; }
    }
}
