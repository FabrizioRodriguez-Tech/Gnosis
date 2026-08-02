using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Business.Models;
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

        public async Task<SesionEnfoque> IniciarSesionAsync(Guid usuarioId, string tipoSesion)
        {
            var nuevaSesion = new SesionEnfoque
            {
                UsuarioId = usuarioId,
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

        public async Task<SesionEnfoqueModel> RegistrarSesionCompletadaAsync(Guid usuarioId, string tipoSesion, int duracionMinutos)
        {
            // Ambas fechas se generan en el servidor a partir de UtcNow, así quedan con Kind=Utc
            // (obligatorio para la columna "timestamp with time zone" de Postgres).
            var fin = DateTime.UtcNow;
            var inicio = fin.AddMinutes(-Math.Max(0, duracionMinutos));

            var sesion = new SesionEnfoque
            {
                UsuarioId = usuarioId,
                FechaInicio = inicio,
                FechaFin = fin,
                DuracionMinutos = duracionMinutos,
                TipoSesion = tipoSesion
            };

            await _sesionRepository.AgregarAsync(sesion);

            return new SesionEnfoqueModel
            {
                Id = sesion.Id,
                FechaInicio = inicio,
                FechaFin = fin,
                DuracionMinutos = sesion.DuracionMinutos,
                TipoSesion = sesion.TipoSesion
            };
        }
    }
}
