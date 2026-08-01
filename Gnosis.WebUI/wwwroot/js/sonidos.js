// Gnosis.WebUI/wwwroot/js/sonidos.js
window.gnosis = window.gnosis || { }
;

// Contexto de audio (singleton)
let audioContext = null;

function getAudioContext()
{
    if (!audioContext)
    {
        try
        {
            audioContext = new(window.AudioContext || window.webkitAudioContext)();
        }
        catch (e)
        {
            console.warn('Web Audio API no soportada');
            return null;
        }
    }
    return audioContext;
}

// Reproducir sonido sintético
gnosis.reproducirSonido = function(tipo) {
    try
    {
        const ctx = getAudioContext();
        if (!ctx) return;

        // IMPORTANTE: Reanudar si está suspendido
        if (ctx.state === 'suspended')
        {
            ctx.resume();
        }

        const oscillator = ctx.createOscillator();
        const gainNode = ctx.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(ctx.destination);

        let frecuencia = 880;
        let duracion = 0.3;
        let volumen = 0.4;

        switch (tipo)
        {
            case 'notificacion':
                frecuencia = 880;
                duracion = 0.3;
                volumen = 0.4;
                break;
            case 'tick':
                frecuencia = 440;
                duracion = 0.05;
                volumen = 0.15;
                break;
            case 'alarma':
                frecuencia = 1000;
                duracion = 0.5;
                volumen = 0.5;
                break;
            default:
                frecuencia = 880;
                duracion = 0.3;
                volumen = 0.3;
        }

        oscillator.type = 'sine';
        oscillator.frequency.value = frecuencia;

        // Envolvente de volumen (para evitar click)
        gainNode.gain.setValueAtTime(0.01, ctx.currentTime);
        gainNode.gain.linearRampToValueAtTime(volumen, ctx.currentTime + 0.01);
        gainNode.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duracion);

        oscillator.start(ctx.currentTime);
        oscillator.stop(ctx.currentTime + duracion);

        // Si es notificación, hacer segundo tono
        if (tipo === 'notificacion')
        {
            setTimeout(() => {
                try
                {
                    const osc2 = ctx.createOscillator();
                    const gain2 = ctx.createGain();
                    osc2.connect(gain2);
                    gain2.connect(ctx.destination);
                    osc2.type = 'sine';
                    osc2.frequency.value = 660;
                    gain2.gain.setValueAtTime(0.01, ctx.currentTime);
                    gain2.gain.linearRampToValueAtTime(volumen, ctx.currentTime + 0.01);
                    gain2.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duracion);
                    osc2.start(ctx.currentTime);
                    osc2.stop(ctx.currentTime + duracion);
                }
                catch (e)
                {
                    console.log('Segundo tono error:', e);
                }
            }, 300);
        }
    }
    catch (e)
    {
        console.log('Audio error:', e);
    }
}
;

// Tick rápido
gnosis.reproducirTick = function() {
    gnosis.reproducirSonido('tick');
}
;

// Alarma múltiple
gnosis.reproducirAlarma = function() {
    const intervalos = [0, 400, 800, 1200];
    intervalos.forEach((delay) => {
        setTimeout(() => {
            gnosis.reproducirSonido('alarma');
        }, delay);
    });
}
;