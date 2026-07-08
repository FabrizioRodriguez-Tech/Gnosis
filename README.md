# Gnosis

Aplicación web de productividad personal construida con **Blazor WebAssembly** y arquitectura **N-Layer**. Combina un temporizador Pomodoro, lista de tareas, reproductor de música y fondos de video ambientales en una sola interfaz minimalista.

---

## Diagramas de arquitectura C4

### Nivel 1 — Contexto

> **Para quién es:** cualquier persona (técnica o no) que quiera entender qué hace el sistema y quién lo usa.
> **Pregunta que responde:** ¿Qué es Gnosis y cómo encaja en el mundo?

```mermaid
C4Context
    title Gnosis — Diagrama de Contexto (Nivel 1)

    Person(usuario, "Usuario", "Estudiante o profesional que quiere gestionar su tiempo y tareas")

    System(gnosis, "Gnosis", "Aplicación web de productividad. Ofrece temporizador Pomodoro, lista de tareas, música de ambiente y fondos de video")

    System_Ext(youtube, "YouTube", "Proporciona videos de fondo en bucle y audio ambiente via iframe embebido")
    System_Ext(groq, "Groq API", "Modelo de lenguaje (Llama 70B) para asistencia inteligente en la organización de tareas")

    Rel(usuario, gnosis, "Usa", "HTTPS / Navegador")
    Rel(gnosis, youtube, "Embebe videos", "iframe / YouTube IFrame API")
    Rel(gnosis, groq, "Consulta IA", "HTTPS / REST")
```

---

### Nivel 2 — Contenedores

> **Para quién es:** desarrolladores que se incorporan al proyecto o revisores técnicos.
> **Pregunta que responde:** ¿Cuáles son las piezas técnicas del sistema y cómo se comunican entre sí?

```mermaid
C4Container
    title Gnosis — Diagrama de Contenedores (Nivel 2)

    Person(usuario, "Usuario", "Accede desde el navegador")

    Container(webui, "Gnosis.WebUI", "Blazor WebAssembly", "Interfaz de usuario. Renderiza los paneles flotantes, el carrusel de fondos, la barra de IA y gestiona el estado local")
    Container(webapi, "Gnosis.WebApi", "ASP.NET Core", "API REST. Expone endpoints para CRUD de tareas y proxy hacia Groq API")
    ContainerDb(db, "Base de datos", "SQL Server / EF Core", "Almacena las tareas y subtareas del usuario")

    System_Ext(youtube, "YouTube", "Videos de fondo y audio")
    System_Ext(groq, "Groq API", "LLM Llama 70B para asistencia")

    Rel(usuario, webui, "Interactúa", "HTTPS")
    Rel(webui, webapi, "Llama endpoints", "HTTP / JSON — puerto 5173")
    Rel(webui, youtube, "Embebe contenido", "iframe")
    Rel(webapi, db, "Lee y escribe", "EF Core")
    Rel(webapi, groq, "Envía consultas", "HTTPS / Bearer token")
```

---

### Nivel 3 — Componentes

> **Para quién es:** el desarrollador que va a modificar o extender el sistema.
> **Pregunta que responde:** ¿Qué hay dentro de cada contenedor y cómo se organizan internamente?

#### Gnosis.WebUI

```mermaid
C4Component
    title Gnosis.WebUI — Componentes internos (Nivel 3)

    Container_Boundary(webui, "Gnosis.WebUI") {
        Component(home, "Home.razor", "Blazor Page", "Página principal. Orquesta todos los paneles flotantes y la barra de IA")
        Component(selector, "SelectorFondos.razor", "Blazor Component", "Carrusel de fondos de video. Gestiona selección, persistencia en localStorage y miniaturas de YouTube")
        Component(pomodoro, "TemporizadorPomodoro.cs", "Singleton Service", "Lógica del temporizador con patrón State (Detenido, Corriendo, Pausado). Emite eventos OnTick")
        Component(tareaproxy, "TareaHttpProxy.cs", "Scoped Service", "Proxy HTTP hacia Gnosis.WebApi. CRUD de tareas y subtareas")
        Component(iaservice, "GnosisIAService.cs", "Scoped Service", "Llama al endpoint api/IA/consultar. Convierte la respuesta en TareaModel o texto según el modo")
        Component(estadofondo, "EstadoFondoActual.cs", "Singleton Service", "Estado compartido del fondo activo. Notifica cambios via evento OnCambio")
        Component(fondopersistence, "FondoPersistenceService.cs", "Scoped Service", "Persiste el fondo elegido en localStorage via JS Interop")
    }

    Rel(home, pomodoro, "Suscribe a OnTick")
    Rel(home, estadofondo, "Suscribe a OnCambio")
    Rel(home, tareaproxy, "CRUD de tareas")
    Rel(home, iaservice, "Envía mensajes del chat")
    Rel(selector, estadofondo, "Actualiza fondo activo")
    Rel(selector, fondopersistence, "Lee y guarda en localStorage")
    Rel(iaservice, tareaproxy, "Crea tareas generadas por IA")
```

#### Gnosis.WebApi

```mermaid
C4Component
    title Gnosis.WebApi — Componentes internos (Nivel 3)

    Container_Boundary(webapi, "Gnosis.WebApi") {
        Component(tareasctrl, "TareasController", "ASP.NET Controller", "CRUD completo de tareas y subtareas. GET, POST, PUT, DELETE en /api/Tareas")
        Component(iactrl, "IAController", "ASP.NET Controller", "Recibe mensajes del chat con historial, construye el prompt para Groq y parsea la respuesta JSON estructurada")
        Component(repos, "Repositories", "EF Core / Infrastructure", "Acceso a datos via patrón Repository. Implementaciones en Gnosis.Infrastructure")
        Component(dbcontext, "GnosisDbContext", "EF Core DbContext", "Mapeo de entidades a tablas SQL Server")
    }

    Rel(tareasctrl, repos, "Delega operaciones")
    Rel(repos, dbcontext, "Usa")
    Rel(iactrl, repos, "Lee tareas si necesita contexto")
```

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend | ASP.NET Core Web API (.NET 8) |
| ORM | Entity Framework Core |
| Base de datos | SQL Server |
| IA | Groq API — `llama-3.3-70b-versatile` |
| Video / Audio | YouTube IFrame API |
| Estilos | Bootstrap 5 + CSS custom |
| Persistencia cliente | localStorage (JS Interop) |

---

## Estructura de la solución

```
Gnosis/
├── Gnosis.Domain/          # Entidades e interfaces
├── Gnosis.Business/        # Modelos, DTOs y servicios de negocio
├── Gnosis.Infrastructure/  # Repositorios, DbContext, migraciones
├── Gnosis.WebApi/          # API REST (puerto 5173)
└── Gnosis.WebUI/           # Frontend Blazor WebAssembly (puerto 44372)
```

---

## Funcionalidades implementadas

- **Fondo de video** — iframe de YouTube en bucle a pantalla completa con carrusel de selección y miniaturas reales. Persiste entre sesiones via localStorage.
- **Panel Pomodoro** — temporizador flotante y arrastrable con modos Enfoque / Descanso Corto / Descanso Largo. Minimizable a lengüeta en el borde de la pantalla.
- **Lista To-Do** — panel flotante y arrastrable con tareas, subtareas, checkboxes y eliminación. Sincronizado con la base de datos via API.
- **Panel Música** — reproductor de audio de YouTube integrado con control de volumen y play/pause.
- **Asistente IA** — chat en barra inferior que conversa en español usando Llama 70B via Groq. Detecta cuándo el usuario quiere crear tareas y las agrega directamente al panel To-Do.
