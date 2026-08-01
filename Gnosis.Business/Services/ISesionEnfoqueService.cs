using System;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;

namespace Gnosis.Business.Services
{
    public interface ISesionEnfoqueService
    {
        Task<SesionEnfoque> IniciarSesionAsync(string tipoSesion);
        Task<SesionEnfoque> FinalizarSesionAsync(Guid sesionId);

        // Registra una sesión ya completada (el Pomodoro del cliente avisa una sola vez al terminar el ciclo)
        Task<SesionEnfoqueModel> RegistrarSesionCompletadaAsync(string tipoSesion, int duracionMinutos);
    }
}