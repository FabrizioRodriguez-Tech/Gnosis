using System;
namespace Gnosis.WebUI
{
    // Modos de sesión disponibles para el Pomodoro
    public enum ModoPomodoro
    {
        Enfoque,
        DescansoCorto,
        DescansoLargo
    }

    // 1. Interfaz abstracta del Estado
    public interface ITemporizadorEstado
    {
        void Iniciar(TemporizadorPomodoro contexto);
        void Pausar(TemporizadorPomodoro contexto);
        void Detener(TemporizadorPomodoro contexto);
        void Tick(TemporizadorPomodoro contexto);
    }

    // 2. Estado Concreto: Detenido
    public class EstadoDetenido : ITemporizadorEstado
    {
        public void Iniciar(TemporizadorPomodoro contexto) => contexto.CambiarEstado(new EstadoCorriendo());
        public void Pausar(TemporizadorPomodoro contexto) { }
        public void Detener(TemporizadorPomodoro contexto) => contexto.RestablecerTiempoPorModo();
        public void Tick(TemporizadorPomodoro contexto) { }
    }

    // 3. Estado Concreto: Corriendo (Activo)
    public class EstadoCorriendo : ITemporizadorEstado
    {
        public void Iniciar(TemporizadorPomodoro contexto) { }
        public void Pausar(TemporizadorPomodoro contexto) => contexto.CambiarEstado(new EstadoPausado());
        public void Detener(TemporizadorPomodoro contexto)
        {
            contexto.RestablecerTiempoPorModo();
            contexto.CambiarEstado(new EstadoDetenido());
        }
        public void Tick(TemporizadorPomodoro contexto)
        {
            if (contexto.Segundos == 0)
            {
                if (contexto.Minutos == 0)
                {
                    contexto.CambiarEstado(new EstadoTerminado());
                    contexto.NotificarFinSesion();
                    return;
                }
                contexto.Minutos--;
                contexto.Segundos = 59;
            }
            else
            {
                contexto.Segundos--;
            }
        }
    }

    // 4. Estado Concreto: Pausado
    public class EstadoPausado : ITemporizadorEstado
    {
        public void Iniciar(TemporizadorPomodoro contexto) => contexto.CambiarEstado(new EstadoCorriendo());
        public void Pausar(TemporizadorPomodoro contexto) { }
        public void Detener(TemporizadorPomodoro contexto)
        {
            contexto.RestablecerTiempoPorModo();
            contexto.CambiarEstado(new EstadoDetenido());
        }
        public void Tick(TemporizadorPomodoro contexto) { }
    }

    // 5. Estado Concreto: Terminado
    public class EstadoTerminado : ITemporizadorEstado
    {
        public void Iniciar(TemporizadorPomodoro contexto)
        {
            contexto.RestablecerTiempoPorModo();
            contexto.CambiarEstado(new EstadoCorriendo());
        }
        public void Pausar(TemporizadorPomodoro contexto) { }
        public void Detener(TemporizadorPomodoro contexto)
        {
            contexto.RestablecerTiempoPorModo();
            contexto.CambiarEstado(new EstadoDetenido());
        }
        public void Tick(TemporizadorPomodoro contexto) { }
    }

    // 6. El Contexto del Temporizador
    public class TemporizadorPomodoro
    {
        private ITemporizadorEstado _estadoActual = new EstadoDetenido();
        private readonly System.Timers.Timer _internalTimer;
        private ModoPomodoro _modoActual = ModoPomodoro.Enfoque;

        // Cuenta los descansos cortos consecutivos completados; al llegar a 2, el siguiente descanso es largo
        private int _descansosCortosSeguidos = 0;

        // [AÑADIDO] Propiedades para almacenar los tiempos configurados por el usuario
        public int DuracionEnfoque { get; set; } = 25;
        public int DuracionDescansoCorto { get; set; } = 5;
        public int DuracionDescansoLargo { get; set; } = 15;

        public int Minutos { get; set; } = 25;
        public int Segundos { get; set; } = 0;

        public ModoPomodoro ModoActual
        {
            get => _modoActual;
            set
            {
                _modoActual = value;
                RestablecerTiempoPorModo();
            }
        }

        public string TiempoFormateado => $"{Minutos:D2}:{Segundos:D2}";
        public string NombreEstadoActual => _estadoActual.GetType().Name.Replace("Estado", "");

        public event Action? OnTick;
        public event Action? OnSesionTerminada;

        public TemporizadorPomodoro()
        {
            _internalTimer = new System.Timers.Timer(1000);
            _internalTimer.Elapsed += (s, e) => EjecutarTick();

            // Inicializa los minutos con la duración de Enfoque configurada
            Minutos = DuracionEnfoque;
        }

        public void CambiarEstado(ITemporizadorEstado nuevoEstado)
        {
            _estadoActual = nuevoEstado;
            if (_estadoActual is EstadoCorriendo) _internalTimer.Start();
            else _internalTimer.Stop();
            OnTick?.Invoke();
        }

        public void Iniciar() => _estadoActual.Iniciar(this);
        public void Pausar() => _estadoActual.Pausar(this);
        public void Detener() => _estadoActual.Detener(this);

        public void CambiarModo(ModoPomodoro nuevoModo)
        {
            _modoActual = nuevoModo;
            RestablecerTiempoPorModo();
            Detener();
            OnTick?.Invoke();
        }

        // [CORREGIDO] Ahora lee dinámicamente de las propiedades configurables
        public void RestablecerTiempoPorModo()
        {
            Minutos = _modoActual switch
            {
                ModoPomodoro.Enfoque => DuracionEnfoque,
                ModoPomodoro.DescansoCorto => DuracionDescansoCorto,
                ModoPomodoro.DescansoLargo => DuracionDescansoLargo,
                _ => DuracionEnfoque
            };
            Segundos = 0;
        }

        private void EjecutarTick() { _estadoActual.Tick(this); OnTick?.Invoke(); }

        // [AÑADIDO] Al terminar una sesión: notifica y avanza automáticamente al siguiente modo,
        // arrancándolo sin intervención del usuario.
        public void NotificarFinSesion()
        {
            OnSesionTerminada?.Invoke();
            AvanzarModoAutomatico();
        }

        private void AvanzarModoAutomatico()
        {
            ModoPomodoro siguienteModo;

            switch (_modoActual)
            {
                case ModoPomodoro.Enfoque:
                    // Después de 2 descansos cortos seguidos, toca descanso largo
                    siguienteModo = _descansosCortosSeguidos >= 2 ? ModoPomodoro.DescansoLargo : ModoPomodoro.DescansoCorto;
                    break;
                case ModoPomodoro.DescansoCorto:
                    _descansosCortosSeguidos++;
                    siguienteModo = ModoPomodoro.Enfoque;
                    break;
                case ModoPomodoro.DescansoLargo:
                    _descansosCortosSeguidos = 0;
                    siguienteModo = ModoPomodoro.Enfoque;
                    break;
                default:
                    siguienteModo = ModoPomodoro.Enfoque;
                    break;
            }

            _modoActual = siguienteModo;
            RestablecerTiempoPorModo();
            CambiarEstado(new EstadoCorriendo());
        }
    }
}