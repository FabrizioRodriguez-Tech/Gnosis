namespace Gnosis.Business.Services;

using Gnosis.Domain.Interfaces;
using System.Net.Http.Json;

public class VideoService : IVideoService
{
    private readonly HttpClient _http;
    private const string ApiKey = "mG5XNYqr1JoxD7IZGhOYvz12uiMQXwaljJNib30zZYwGI7zzJpy28qnp";

    public VideoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetRandomFocusVideoAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.pexels.com/videos/search?query=nature&per_page=15");
        request.Headers.Add("Authorization", ApiKey);

        var response = await _http.SendAsync(request);
        var data = await response.Content.ReadFromJsonAsync<PexelsResponse>();

        var randomVideo = data.videos[new Random().Next(data.videos.Count)];
        return randomVideo.video_files.First(f => f.file_type == "video/mp4").link;
    }
}

// Clases auxiliares para deserializar la respuesta de Pexels
public class PexelsResponse { public List<Video> videos { get; set; } }
public class Video { public List<VideoFile> video_files { get; set; } }
public class VideoFile { public string link { get; set; } public string file_type { get; set; } }