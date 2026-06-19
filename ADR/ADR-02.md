# ADR-02: Definición y Adopción de Vistas Arquitectónicas

## Estado
Propuesto

## Contexto
El Proyecto Gnosis es un sistema enfocado en el soporte cognitivo y la gestión de la productividad, diseñado bajo una arquitectura en capas (N-Layer) utilizando .NET 8 y Blazor WebAssembly. 

Para garantizar la mantenibilidad del sistema, facilitar la integración de futuros desarrolladores y cumplir con los estándares de diseño formal, es necesario documentar la estructura, el comportamiento y el entorno de despliegue de la aplicación desde múltiples perspectivas técnicas coherentes entre sí.

## Decisión
Se adopta el enfoque de documentación mediante cuatro vistas arquitectónicas fundamentales (Lógica, Desarrollo, Procesos y Despliegue). Los diagramas asociados se integran directamente en la documentación técnica utilizando la sintaxis de Mermaid para asegurar su compatibilidad y renderizado nativo dentro del repositorio de GitHub.

A continuación, se detallan las cuatro vistas aplicadas formalmente al diseño actual de Gnosis.

---

## 1. Vista Lógica
Esta vista describe la descomposición del sistema en módulos funcionales y asigna las responsabilidades correspondientes a cada componente de la arquitectura.

### Tabla de Responsabilidades

| Módulo / Capa | Componente | Responsabilidad Principal | Relación / Dependencia |
| :--- | :--- | :--- | :--- |
| **Gnosis.Domain** | `Entities` | Define las estructuras de datos esenciales (`Tarea`, `Nota`, `SesionPomodoro`) libres de lógica tecnológica o dependencias de frameworks. | Ninguna (Núcleo independiente). |
| **Gnosis.Domain** | `Interfaces` | Establece los contratos de persistencia (`IRepository`, `ITareaRepository`) para abstraer el acceso a datos. | Depende de `Entities`. |
| **Gnosis.Business** | `Services` | Orquesta los casos de uso principales, aplicando las reglas de negocio para el temporizador Pomodoro y el desglose de tareas. | Consume interfaces de `Gnosis.Domain`. |
| **Gnosis.Infrastructure**| `Repositories`| Implementa el acceso a datos real y la persistencia de las entidades en el almacenamiento físico. | Implementa contratos de `Gnosis.Domain` y provee servicios a `Gnosis.Business`. |
| **Gnosis.WebUI** | `Pages / Components`| Gestiona la interfaz de usuario interactiva (Entorno Zen, reproductores de audio y vistas de usuario). | Consume servicios expuestos por `Gnosis.Business`. |

## Vista Lógica

<img width="6081" height="1427" alt="Vista Lógica" src="https://github.com/user-attachments/assets/87369d04-d1e6-43db-a75a-793b8dc3c675" />

## Vista de Desarrollo
<img width="7430" height="3887" alt="Vista Desarrollo" src="https://github.com/user-attachments/assets/3d05c47d-8568-4212-bd21-55f2e1f39b4c" />

## Vista de Procesos 
<img width="8191" height="2612" alt="Vista procesos" src="https://github.com/user-attachments/assets/3fba58c3-8a9a-4464-b17e-036b24a2cf59" />

## Vista de despliegue
  <img width="4132" height="3405" alt="Vista Despliegue" src="https://github.com/user-attachments/assets/9bd1ce85-424a-4a85-93f3-50c3901389fa" />
