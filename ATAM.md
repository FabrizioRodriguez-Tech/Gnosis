# Evaluación ATAM — Proyecto Gnosis

Evaluación de la arquitectura mediante el método **ATAM (Architecture Tradeoff Analysis Method)**. Se documentan un riesgo, un trade-off y un punto de sensibilidad, cada uno justificado con una decisión y un incidente real ocurridos durante el desarrollo de Gnosis (ver `ADR-01.md`, `ADR-03.md` y `ADR-004.md`).

## Estado
Aceptado

---

## 1. Riesgo

**Riesgo:** la ausencia de validaciones de integridad referencial a nivel de repositorio, consecuencia directa de haber adoptado un repositorio genérico único (`IRepository<T>`, ADR-004 §2), permite que la capa de negocio inserte una entidad con una clave foránea (`TareaId`) que no existe todavía en la base de datos.

**Decisión arquitectónica relacionada:** ADR-004, decisión 2 — estandarizar el acceso a datos exclusivamente a través de `IRepository<T>` / `EfRepository<T>`, sin repositorios específicos por entidad ni validaciones de dominio adicionales antes de persistir.

**Evidencia real:** durante el desarrollo de la Agenda, la interfaz de Blazor genera un `Guid` en el cliente para actualizar la UI de forma optimista antes de confirmar la persistencia (`CrearNuevaTarea` en `Home.razor`). Si la llamada `TareaProxy.CrearTareaAsync(...)` fallaba silenciosamente, la tarea quedaba visible en pantalla con un `Id` que nunca se insertó en PostgreSQL. Al vincular un bloque de la Agenda a esa tarea "fantasma", el `INSERT` sobre `BloquesTiempo` disparaba la excepción `FK_BloquesTiempo_Tareas_TareaId`, reproducida varias veces en distintas sesiones de prueba.

**Mitigación aplicada:** se revirtió el agregado optimista en el cliente cuando la persistencia falla (`tareas.Remove(nueva)` + aviso visible en UI), eliminando la posibilidad de vincular una tarea fantasma desde la interfaz. **Mitigación pendiente:** el riesgo estructural persiste a nivel de API — `BloquesTiempoController` no valida hoy que el `TareaId` recibido exista antes de delegar en `BloqueTiempoService`; cualquier otro cliente de la API (no solo `Gnosis.WebUI`) podría reproducir el mismo error.

---

## 2. Trade-off

**Trade-off:** Modificabilidad y testabilidad (ganadas por la arquitectura N-Layer estricta) **vs.** velocidad de desarrollo por feature (perdida por el número de proyectos que hay que tocar para cada capacidad nueva).

**Decisión arquitectónica relacionada:** ADR-01 y ADR-03 — cinco capas independientes (Domain, Business, Infrastructure, WebApi, WebUI) con inversión de dependencias hacia el dominio.

**Evidencia real:** implementar la Agenda (ADR-004 §3) requirió crear o modificar archivos en los cinco proyectos: `Gnosis.Domain` (entidad `BloqueTiempo`), `Gnosis.Business` (`BloqueTiempoModel`, `IBloqueTiempoService`, `BloqueTiempoService`), `Gnosis.Infrastructure` (mapeo en `GnosisDbContext`, migración EF Core), `Gnosis.WebApi` (`BloquesTiempoController`) y `Gnosis.WebUI` (`BloqueTiempoHttpProxy`, `PanelAgenda.razor`). El mismo patrón se repitió íntegro para el Dashboard.

**Por qué se acepta el trade-off:** a cambio de ese costo por feature, ninguna de las dos features nuevas introdujo una regresión en Pomodoro, Audio o IA durante el desarrollo — el aislamiento por capas cumplió su promesa de contener el radio de impacto de cada cambio, que era el requisito explícito del proyecto ("cuidar los detalles y no arruinar lo demás").

---

## 3. Punto de sensibilidad

**Punto de sensibilidad:** la arquitectura de Gnosis es altamente sensible a **quién genera el identificador (`Guid`) de una entidad nueva** — el cliente (Blazor WASM, para permitir actualización optimista de UI) o el servidor.

**Decisión arquitectónica relacionada:** `Gnosis.WebUI` genera `Id = Guid.NewGuid()` en el cliente al crear una tarea, para pintarla en el `To-Do` inmediatamente sin esperar la respuesta HTTP. Ese mismo `Id` debe viajar hasta `TareaService.CrearTareaRaizAsync` y persistirse tal cual en PostgreSQL para que ambos lados queden sincronizados.

**Evidencia real:** en una primera versión, `CrearTareaRaizAsync` ignoraba el `Id` recibido y generaba uno propio (`Id = Guid.NewGuid()` también en el servidor), y `CrearTareaRequest` ni siquiera exponía un campo `Id`. Ese único parámetro de diseño —quién es la autoridad del identificador— bastó para producir, de forma reproducible, la violación de llave foránea `FK_BloquesTiempo_Tareas_TareaId` cada vez que se vinculaba una tarea recién creada a un bloque de Agenda. Corregir un solo punto (pasar y respetar el `Guid` del cliente de extremo a extremo: `CrearTareaRequest.Id` → `ITareaService.CrearTareaRaizAsync(Guid? id, ...)` → `TareaService`) resolvió la clase completa de errores, sin tocar ninguna otra capa.

**Atributos de calidad afectados:** integridad de datos (consistencia entre el estado optimista del cliente y el estado persistido) y capacidad de respuesta percibida (UX) — ambos dependen de ese único parámetro de diseño, lo que lo convierte en un punto de sensibilidad clásico según ATAM: un cambio pequeño y localizado con un impacto desproporcionado sobre la arquitectura.

---

## Resumen

| Categoría | Hallazgo | Decisión asociada |
| :--- | :--- | :--- |
| Riesgo | Falta de validación de integridad referencial en el repositorio genérico | ADR-004 §2 |
| Trade-off | Modificabilidad/testabilidad vs. velocidad de desarrollo por feature | ADR-01 / ADR-03 |
| Sensibilidad | Autoridad del `Guid` (cliente vs. servidor) en la creación de entidades | Flujo `CrearTareaRaizAsync` |
