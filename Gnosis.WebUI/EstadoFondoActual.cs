// Ubicación real: Gnosis.WebUI/EstadoFondoActual.cs
using Gnosis.Business.Models;

namespace Gnosis.WebUI
{
    public class EstadoFondoActual
    {
        private FondoVideo _fondoActivo = CatalogoFondos.Predeterminado;

        public FondoVideo FondoActivo => _fondoActivo;

        public event Action? OnCambio;

        public void EstablecerFondo(FondoVideo fondo)
        {
            if (_fondoActivo.Id == fondo.Id) return;
            _fondoActivo = fondo;
            OnCambio?.Invoke();
        }
    }
}