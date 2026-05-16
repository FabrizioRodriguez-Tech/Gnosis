# ADR-01: Arquitectura N-Layer con Desacoplamiento de Contextos de Usuario

| Campo | Valor |
| :--- | :--- |
| **Autor** | Alexander Fabrizio Rodriguez Pérez |
| **Fecha** | 15/05/2026 |
| **Estado** | Propuesto |

## Contexto
El entorno académico actual exige a los estudiantes gestionar volúmenes de información masivos y múltiples tareas simultáneas, lo que frecuentemente deriva en sobrecarga cognitiva y parálisis por análisis. Los gestores de tareas convencionales se limitan a listar pendientes, pero no asisten al usuario en la transición crítica entre la planificación y la ejecución.

Gnosis se construye para resolver esta brecha mediante un sistema de soporte cognitivo que ataca dos frentes:

1. **La fragmentación del enfoque:** Proporcionando un motor de desglose que transforma objetivos ambiguos en micro-pasos atómicos (manuales o asistidos por IA), reduciendo la fricción inicial para comenzar a trabajar.

2. **La degradación de la atención:** Implementando un entorno de inmersión digital que aísla las herramientas de estudio de las distracciones de la gestión, optimizando el tiempo de trabajo profundo (Deep Work).

La solución requiere una infraestructura que garantice la integridad de los datos académicos y la disponibilidad del servicio incluso en condiciones de baja conectividad, asegurando que el flujo de pensamiento del estudiante no se vea interrumpido por limitaciones técnicas.

## Decisión
He decidido implementar un estilo arquitectónico **N-Layer (Multicapa)** utilizando **.NET** y **Blazor**. La aplicación se dividirá en cuatro proyectos: **Domain**, **Business**, **Infrastructure** y **WebUI**. Además, se aplicará un desacoplamiento de la interfaz mediante **Layouts Dinámicos** para separar el contexto de "Gestión" del contexto de "Ejecución (Modo Zen)".

## ¿Por qué?
1. **N-Layer:** Permite aislar la lógica de estudio (desglose de tareas) de los detalles de implementación (SQL Server o APIs de IA). Esto facilita el mantenimiento y el testeo unitario.
2. **Layouts Dinámicos:** Resuelve el problema de la distracción. Al cambiar físicamente la interfaz entre planificación y trabajo profundo, reducimos la carga cognitiva del usuario, aplicando principios de UX orientados a la psicología del aprendizaje.
3. **Blazor:** Permite reutilizar modelos de C# en el frontend y backend, acelerando el desarrollo dentro del ecosistema .NET solicitado.

## Alternativas consideradas

| Alternativa | Por qué la descarté |
| :--- | :--- |
| **Arquitectura Monolítica** | Aunque es rápida de iniciar, mezcla responsabilidades y dificulta el crecimiento del sistema, violando los principios de Clean Code. |
| **Arquitectura de Microservicios** | Agregaría una complejidad excesiva en la comunicación entre servicios y el despliegue. |
| **Bases de Datos NoSQL (MongoDB)** | Se descartó porque el modelo de Gnosis es altamente relacional (Materias -> Notas -> Tareas -> Pasos). SQL Server garantiza la integridad de estos datos. |

## Consecuencias

### Beneficios obtenidos
* **Consecuencia técnica:** Alta mantenibilidad y testeabilidad. Es posible reemplazar el motor de IA o cambiar el motor de base de datos sin afectar la lógica de los micro-pasos ni la interfaz de usuario.
* **Consecuencia sobre el proceso:** Claridad en el flujo de desarrollo. Al separar las capas, se puede trabajar en el diseño de la persistencia y en la lógica de negocio de forma paralela y organizada.

### Limitaciones y riesgos asumidos
* **Limitación técnica:** Mayor complejidad en la gestión del estado de la aplicación. Mantener el temporizador Pomodoro y el flujo de audio activos durante los cambios de layout requiere una implementación avanzada de servicios inyectados con ciclo de vida adecuado.
* **Deuda o riesgo:** Al implementar capacidades offline (PWA) bajo una arquitectura N-Layer, se asume un incremento en la complejidad de la lógica de sincronización entre el almacenamiento local del navegador y la base de datos SQL Server.

## Diagrama de la Solución
<img width="969" height="512" alt="UML Gnosis drawio" src="https://github.com/user-attachments/assets/8bf9aef5-b491-42b8-b578-e5288af91933" />
