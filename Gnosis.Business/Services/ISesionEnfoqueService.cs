using System;
using System.Threading.Tasks;
using Gnosis.Domain.Entities;

namespace Gnosis.Business.Services
{
    
    internal interface ISesionEnfoqueService
    {
        Task<SesionEnfoque> IniciarSesionAsync(string tipoSesion);
        Task<SesionEnfoque> FinalizarSesionAsync(Guid sesionId);
    }
}