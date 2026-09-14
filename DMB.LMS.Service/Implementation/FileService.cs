using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dmb.Lms.Model.Dtos;
using Dmb.Lms.Service.Interface;
using Microsoft.Extensions.Configuration;

namespace Dmb.Lms.Service.Implementation;

public class FileService : IFileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public FileService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<FileUploadResult> UploadAsync(string prefix, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default)
    {
        var (url, key) = Credentials();
        var safe = $"{prefix.Trim('/')}/{Guid.NewGuid():N}-{Sanitize(fileName)}";
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{url}/storage/v1/object/lms/{safe}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        req.Content = new StreamContent(content);
        req.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        var res = await client.SendAsync(req, cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Storage upload failed: {res.StatusCode} {body}");
        }

        return new FileUploadResult { StoragePath = safe, SignedUrl = await SignAsync(safe, cancellationToken) };
    }

    public async Task<string?> SignAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return null;
        var (url, key) = Credentials();
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{url}/storage/v1/object/sign/lms/{storagePath.TrimStart('/')}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        req.Content = new StringContent(JsonSerializer.Serialize(new { expiresIn = 3600 }), Encoding.UTF8, "application/json");
        var res = await client.SendAsync(req, cancellationToken);
        if (!res.IsSuccessStatusCode) return null;
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
        if (!doc.RootElement.TryGetProperty("signedURL", out var signed)) return null;
        var path = signed.GetString();
        if (string.IsNullOrWhiteSpace(path)) return null;
        return path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : $"{url}{path}";
    }

    private (string Url, string Key) Credentials()
    {
        var url = (_configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
        var key = _configuration["Supabase:ServiceRole"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Supabase storage is not configured.");
        }

        return (url, key);
    }

    private static string Sanitize(string fileName)
    {
        var name = Path.GetFileName(fileName);
        return string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_'));
    }
}
