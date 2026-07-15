# Refactorización de Code Smells — Proyecto Gnosis

## Code Smells identificados

### Code Smell #1 — God Class en `Home.razor`

**Descripción:**  
`Home.razor` concentraba en un solo archivo la lógica y la UI de cinco responsabilidades completamente distintas: temporizador Pomodoro, lista To-Do, reproductor de audio, asistente IA y gestión del fondo de video. El archivo superaba las 350 líneas de código mezclando estado, eventos, llamadas a servicios externos y helpers de CSS en un único componente.

**Por qué es un problema:**  
Una God Class es difícil de mantener porque cualquier cambio en una funcionalidad (por ejemplo, ajustar el reproductor de audio) obliga a navegar por código de todas las demás. Además, imposibilita reutilizar componentes de forma independiente y hace que los errores sean más difíciles de aislar.

**Evidencia antes de la refactorización:**  
`Home.razor` contenía todos estos bloques en un solo archivo:
- Variables de estado: `audioUrl`, `audioCargado`, `audioReproduciendo`, `audioVolumen`, `audioError`, `iaExpandida`, `iaCargando`, `iaMensajeActual`, `iaMensajes`
- Métodos de audio: `CargarAudio()`, `TogglePlayPause()`, `CambiarVolumen()`, `ExtraerVideoId()`
- Métodos de IA: `EnviarMensajeIA()`, `HandleIAKeyUp()`
- UI de audio y UI de IA embebidas directamente en el HTML del componente principal

---

### Code Smell #2 — Long Method en `IAController.Consultar()`

**Descripción:**  
El método `Consultar()` en `Gnosis.WebApi/Controllers/IAController.cs` tiene aproximadamente 100 líneas y realiza ocho responsabilidades distintas en un solo bloque: validación del request, construcción del historial, armado del system prompt, llamada HTTP a Groq, lectura de la respuesta, limpieza de markdown, parsing del JSON y conversión de DTOs.

**Por qué es un problema:**  
Un Long Method viola el principio de responsabilidad única. No se puede testear ninguna parte de forma aislada, y cualquier cambio en el prompt o en el parsing obliga a leer las 100 líneas completas para entender el contexto.

**Nota:** Este code smell fue identificado pero no refactorizado en esta entrega — queda documentado como deuda técnica en `ADR-001-deuda-tecnica.md`.

---

## Refactorización aplicada — Extract Class sobre `Home.razor`

**Técnica:** Extract Class  
**Patrón aplicado:** Separación de responsabilidades en componentes Blazor independientes

### Archivos creados

| Archivo | Responsabilidad extraída |
|---|---|
| `Gnosis.WebUI/Components/PanelAudio.razor` | Toda la lógica y UI del reproductor de audio |
| `Gnosis.WebUI/Components/PanelIA.razor` | Toda la lógica y UI del asistente IA |

### Estructura antes de la refactorización

```
Gnosis.WebUI/
└── Pages/
    └── Home.razor   ← ~350 líneas
                        ├── Lógica Pomodoro
                        ├── Lógica To-Do
                        ├── Lógica Audio (CargarAudio, TogglePlayPause, CambiarVolumen, ExtraerVideoId)
                        ├── Lógica IA (EnviarMensajeIA, iaMensajes, iaCargando, MensajeChat)
                        └── Helpers CSS (BordeMinimizado, EstiloPanelMinimizado)
```

### Estructura después de la refactorización

```
Gnosis.WebUI/
├── Pages/
│   └── Home.razor           ← ~230 líneas (solo orquestador)
│                               ├── Lógica Pomodoro
│                               ├── Lógica To-Do
│                               ├── Callback AgregarTareasDesdeIA
│                               └── Helpers CSS
└── Components/
    ├── SelectorFondos.razor ← sin cambios
    ├── PanelAudio.razor     ← NUEVO — lógica de audio extraída
    └── PanelIA.razor        ← NUEVO — lógica de IA extraída
```

### Cómo se comunican los componentes después de la refactorización

**`PanelAudio` → `Home`:** el arrastre del panel usa `EventCallback<MouseEventArgs> OnArrastre` para notificar a `Home` cuando el usuario hace mousedown, ya que `Home` es quien mantiene la referencia `_dotnetRef` necesaria para el JS de arrastre.

**`PanelIA` → `Home`:** cuando la IA crea tareas, `PanelIA` notifica a `Home` via `EventCallback<List<TareaModel>> OnTareasCreadas`, y `Home` las agrega a la lista To-Do y las persiste en la API.

Este patrón de `EventCallback` es la forma idiomática de Blazor para comunicación hijo → padre sin crear acoplamiento de tipo entre componentes.

---

## Commits de la refactorización

| Commit | Descripción |
|---|---|
| Commit 1 (antes) | `Home.razor` original con God Class — todas las responsabilidades mezcladas |
| Commit 2 (después) | `refactor: extract class - PanelAudio y PanelIA extraídos de Home.razor (God Class)` |

El diff entre ambos commits muestra la reducción de `Home.razor` de ~350 a ~230 líneas y la creación de `PanelAudio.razor` y `PanelIA.razor`.

---

## Comportamiento del sistema

El comportamiento del sistema es idéntico antes y después de la refactorización:

- El panel de audio arrastra, minimiza y reproduce URLs de YouTube igual que antes.
- El chat de IA conversa y crea tareas en el To-Do igual que antes.
- El Pomodoro, la lista To-Do y el selector de fondos no fueron modificados.

La refactorización fue puramente estructural — ninguna funcionalidad fue alterada.
