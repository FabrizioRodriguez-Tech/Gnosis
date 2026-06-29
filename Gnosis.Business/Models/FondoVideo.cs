// Ubicación real: Gnosis.Business/Models/FondoVideo.cs
namespace Gnosis.Business.Models
{
    public class FondoVideo
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string YoutubeVideoId { get; set; } = string.Empty;
        public string ColorPlaceholder { get; set; } = "#2a2a2a";

        public string ThumbnailUrl => $"https://img.youtube.com/vi/{YoutubeVideoId}/mqdefault.jpg";
    }
}