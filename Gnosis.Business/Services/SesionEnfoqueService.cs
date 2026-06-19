using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Domain.Entities;
using Gnosis.Domain.Interfaces;

namespace Gnosis.Business.Services
{
    
    internal class SesionEnfoqueService : ISesionEnfoqueService
    {
        private readonly IRepository<SesionEnfoque> _sesionRepository;

        public SesionEnfoqueService(IRepository<SesionEnfoque> sesionRepository)
        {
            _sesionRepository = sesionRepository;
        }

        public async Task<SesionEnfoque> IniciarSesionAsync(string tipoSesion)
        {
            var nuevaSesion = new SesionEnfoque
            {
                TipoSesion = tipoSesion,
                FechaInicio = DateTime.UtcNow
            };

            await _sesionRepository.AgregarAsync(nuevaSesion);
            return nuevaSesion;
        }

        public async Task<SesionEnfoque> FinalizarSesionAsync(Guid sesionId)
        {
            var sesion = await _sesionRepository.GetByIdAsync(sesionId);
            if (sesion == null) throw new KeyNotFoundException("La sesión de enfoque no existe.");

            sesion.FechaFin = DateTime.UtcNow;
            
            sesion.DuracionMinutos = (int)(sesion.FechaFin.Value - sesion.FechaInicio).TotalMinutes;

            await _sesionRepository.ActualizarAsync(sesion);
            return sesion;
        }
    }
}