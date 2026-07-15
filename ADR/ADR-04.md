# ADR-01: Documentación de Deuda Técnica — Proyecto Gnosis

**Estado:** Identificada, pendiente de resolución  

---

## Contexto

Durante el desarrollo de Gnosis se tomaron decisiones de implementación rápida para cumplir con fechas de entrega. Dos de estas decisiones generaron deuda técnica real que afecta la seguridad, mantenibilidad y escalabilidad del sistema.

---

## Deuda Técnica #1 — API Key de Groq expuesta en archivo de configuración

### Qué es

La clave de autenticación de la API de Groq (`GroqApiKey`) está escrita directamente en `Gnosis.WebApi/appsettings.json` como un valor de texto plano:

```json
{
  "GroqApiKey": "gsk_xxxxxxxxxxxxxxxxxxxx"
}
```

Este archivo forma parte del repositorio de Git, lo que significa que la credencial queda expuesta en el historial de commits y es visible para cualquier persona con acceso al repositorio.

### Por qué existe

Se tomó la decisión consciente de escribir la key directamente en el archivo de configuración para agilizar el desarrollo e integrar la funcionalidad de IA lo antes posible. En el momento de la implementación, el objetivo era validar que la llamada a Groq funcionara correctamente antes de preocuparse por la gestión segura de credenciales.

### Costo de no pagarla

- Cualquier persona con acceso al repositorio puede usar la API key para hacer llamadas a Groq a nombre del proyecto, generando costos no autorizados.
- Si el repositorio se hace público (por ejemplo, para un portafolio), la key queda expuesta inmediatamente.
- GitHub y otras plataformas tienen escáneres automáticos de secrets que pueden revocar o alertar sobre keys expuestas.
- Una vez que la key aparece en el historial de Git, eliminarla del archivo actual no es suficiente — el historial completo debe ser reescrito con `git filter-branch` o `git filter-repo`.
- Si la key se rota por este motivo, el sistema de IA deja de funcionar hasta que se actualice manualmente en todos los entornos.

### Propuesta de solución

Aplicar **Externalize Configuration** usando el sistema de User Secrets de .NET para desarrollo local y variables de entorno para producción:

**Paso 1 — Inicializar User Secrets en Gnosis.WebApi:**
```bash
dotnet user-secrets init
dotnet user-secrets set "GroqApiKey" "gsk_xxxxxxxxxxxxxxxxxxxx"
```

**Paso 2 — Limpiar appsettings.json:**
```json
{
  "GroqApiKey": ""
}
```

**Paso 3 — En producción**, configurar la variable de entorno:
```bash
export GroqApiKey="gsk_xxxxxxxxxxxxxxxxxxxx"
```

El código en `IAController` no requiere ningún cambio porque `_config.GetValue<string>("GroqApiKey")` ya lee automáticamente de User Secrets en desarrollo y de variables de entorno en producción, en ese orden de prioridad.

**Paso 4 — Agregar al .gitignore:**
```
**/secrets.json
**/appsettings.Development.json
```

---

## Deuda Técnica #2 — Long Method en IAController.Consultar()

### Qué es

El método `Consultar()` en `Gnosis.WebApi/Controllers/IAController.cs` tiene aproximadamente 100 líneas y realiza demasiadas responsabilidades en un solo bloque de código:

1. Valida el request entrante
2. Construye la lista de mensajes con el historial para Groq
3. Arma el system prompt completo (40+ líneas de texto)
4. Realiza la llamada HTTP a la API de Groq
5. Parsea el JSON de respuesta
6. Limpia y extrae el contenido de la respuesta
7. Convierte los DTOs de Groq al modelo interno (`TareaIADto`)
8. Maneja errores en múltiples niveles

Este es un **Long Method** que viola el principio de responsabilidad única (SRP): un método no debería hacer todo esto al mismo tiempo.

### Por qué existe

Durante el desarrollo de la funcionalidad de IA se priorizó hacer que funcionara rápidamente en un solo lugar para poder iterar y ajustar el prompt y el parsing sin tener que navegar entre múltiples archivos. La lógica creció incrementalmente — primero fue solo la llamada, luego se añadió el parsing defensivo de subtareas, luego el historial, luego la limpieza de markdown — sin que en ningún momento se hiciera una pausa para refactorizar.

### Costo de no pagarla

- Cualquier cambio en el prompt (ajustar instrucciones al modelo) obliga a navegar por un método de 100 líneas para encontrar la cadena correcta.
- Si se quiere cambiar el proveedor de IA (por ejemplo, migrar de Groq a OpenRouter o Gemini), hay que reescribir todo el método porque la lógica de negocio (detección de modo, conversión de tareas) está mezclada con la lógica de infraestructura (llamada HTTP, headers de autenticación).
- Los tests unitarios son prácticamente imposibles de escribir sobre este método porque no se puede aislar ninguna de sus responsabilidades sin ejecutar todas las demás.
- Cuando el parsing falla (como ocurrió durante el desarrollo cuando Groq devolvía subtareas como objetos en vez de strings), depurar el error requiere leer el método completo para entender el flujo.

### Propuesta de solución

Aplicar **Extract Method** para separar las responsabilidades en métodos privados cohesivos, y **Extract Class** para separar la lógica de comunicación con Groq de la lógica de conversión de DTOs:

**Estructura propuesta:**

```
Gnosis.WebApi/
├── Controllers/
│   └── IAController.cs          ← solo recibe el request y coordina
├── Services/
│   └── GroqService.cs           ← responsable de llamar a Groq API
└── Mappers/
    └── IAResponseMapper.cs      ← responsable de convertir la respuesta a DTOs
```

**IAController** quedaría reducido a:
```csharp
[HttpPost("consultar")]
public async Task<IActionResult> Consultar([FromBody] IARequest request)
{
    var respuestaGroq = await _groqService.ConsultarAsync(request);
    var resultado = _mapper.MapearRespuesta(respuestaGroq);
    return Ok(resultado);
}
```

**GroqService** encapsula la construcción del prompt, la llamada HTTP y el manejo de errores de red.

**IAResponseMapper** encapsula el parsing del JSON, la limpieza de markdown y la conversión de `TareaIADto`.

Esta separación permitiría testear cada pieza de forma independiente y cambiar el proveedor de IA modificando únicamente `GroqService` sin tocar el controller ni el mapper.

---

## Declaración de uso de IA

Durante el desarrollo del proyecto Gnosis se utilizó IA como asistente y guía al momento de solucionar errores como:

- Diseño e implementación de componentes Blazor WebAssembly
- Corrección de código en C#, Razor y CSS

El uso de IA fue supervisado en todo momento — cada fragmento de código fue revisado, adaptado al contexto específico del proyecto e integrado manualmente por el desarrollador. La IA no reemplazó el criterio de diseño ni la toma de decisiones arquitectónicas, sino que actuó como herramienta de apoyo para acelerar la implementación y detectar problemas que de otro modo habrían requerido más tiempo de investigación.
