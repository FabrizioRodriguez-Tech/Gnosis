// Ubicación real: Gnosis.Business/Models/CatalogoFondos.cs
namespace Gnosis.Business.Models
{
    public static class CatalogoFondos
    {
        public static readonly List<FondoVideo> Disponibles = new()
        {
            new FondoVideo { Id = "lluvia-tokio",      Nombre = "Lluvia en Tokio",        YoutubeVideoId = "DvfSPb8VWC4", ColorPlaceholder = "#3a4a5a" },
            new FondoVideo { Id = "fondo-2",            Nombre = "Luxury Miami Apartment",                YoutubeVideoId = "QUqhgZjrrsE", ColorPlaceholder = "#4a5a6a" },
            new FondoVideo { Id = "carretera-lluvia",   Nombre = "Carretera con lluvia",   YoutubeVideoId = "azYAiBWRZA0", ColorPlaceholder = "#2a3a4a" },
            new FondoVideo { Id = "fondo-4",            Nombre = "NewYork Skyline",                YoutubeVideoId = "Qe0aZ26eknU", ColorPlaceholder = "#3a3a5a" },
            new FondoVideo { Id = "shibuya-noche",      Nombre = "Caminata en Shibuya",    YoutubeVideoId = "2y2Z06hVmWE", ColorPlaceholder = "#2a2a3a" },
        };

        public static FondoVideo Predeterminado => Disponibles[0];

        public static FondoVideo? BuscarPorId(string id) =>
            Disponibles.FirstOrDefault(f => f.Id == id);
    }
}