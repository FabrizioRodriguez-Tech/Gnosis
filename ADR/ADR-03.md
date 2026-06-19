# ADR-03: Selección y Justificación del Estilo Arquitectónico

## Estado
Propuesto

## Contexto
El Proyecto Gnosis requiere un marco estructural que soporte el desarrollo de herramientas de soporte cognitivo, gestión de tareas desglosadas y optimización del enfoque (como temporizadores Pomodoro y reproducción de audio). 

Al tratarse de un sistema que procesa reglas de negocio específicas y requiere un consumo eficiente de recursos en el cliente, es fundamental elegir un estilo arquitectónico que mitigue el acoplamiento, facilite las pruebas unitarias y permita la evolución del software sin reconstruir los componentes de presentación.

## Decisión
Se selecciona el estilo **Arquitectura en Capas (N-Layer Architecture)**, estructurado de forma estricta en cuatro niveles independientes: Dominio, Negocio (Aplicación), Infraestructura y Presentación (WebUI con Blazor WebAssembly). 

Esta elección garantiza que el flujo de dependencias se dirija exclusivamente hacia el centro (Inversión de Dependencias), aislando las reglas del negocio de los mecanismos de persistencia y de la interfaz de usuario.

---

## Justificación Técnica
La Arquitectura en Capas resuelve las necesidades de Gnosis debido a los siguientes factores:

1. **Separación de Intereses (Separation of Concerns):** La lógica de temporizadores o el procesamiento del árbol de subtareas pertenecen estrictamente a la capa de Negocio. La lógica de cómo se pintan en el "Entorno Zen" pertenece a la capa WebUI. Esto evita el código espagueti y facilita el mantenimiento local.
2. **Abstracción del Almacenamiento:** En lugar de depender de una base de datos rígida, el Dominio define interfaces. La Infraestructura las implementa, permitiendo cambiar el almacenamiento de datos (LocalStorage, memoria o Azure SQL) sin afectar al resto del sistema.
3. **Facilidad de Pruebas (Testability):** Al estar desacoplada, la lógica de desglose de tareas y las reglas cognitivas pueden someterse a pruebas unitarias aisladas sin necesidad de levantar la interfaz de usuario o conexiones de red reales.

---

## Alternativas Consideradas y Descartadas

### 1. Arquitectura de Microservicios
* **Por qué se consideró:** Ofrece un aislamiento total y escalabilidad independiente para cada funcionalidad (un servicio para Pomodoro, otro para Notas, otro para Tareas).
* **Por qué se descartó:** Introduce una complejidad técnica y latencia de red innecesarias para el alcance actual del proyecto. Gnosis se beneficia de una solución modular dentro de un mismo dominio de ejecución (Monolito Modular), reduciendo costos de infraestructura y sobrecarga de red en el cliente.

### 2. Arquitectura Orientada a Eventos (Event-Driven)
* **Por qué se consideró:** Permite una reactividad alta, ideal para el manejo de alertas de temporizadores y cambios de estado en tiempo real.
* **Por qué se descartó:** La sobrecarga de configurar un bus de eventos (Event Bus) o brokers de mensajería supera los beneficios requeridos para una aplicación de productividad individual en su etapa inicial. El paso de mensajes local y el manejo de estados nativo de Blazor resuelven la interactividad sin añadir infraestructura compleja.

### 3. Arquitectura Hexagonal (Ports and Adapters)
* **Por qué se consideró:** Ofrece un aislamiento robusto del núcleo mediante puertos y adaptadores, ideal para sistemas con múltiples puntos de entrada y salida externos.
* **Por qué se descartó:** Aunque comparte principios con la arquitectura N-Layer adoptada, la estructura de capas tradicional en .NET cumple eficientemente con los objetivos de abstracción del proyecto sin el exceso de archivos de mapeo y traducción jerárquica que exige la arquitectura hexagonal estricta.

---

## Diagrama del Estilo Arquitectónico Aplicado

<img width="5182" height="2930" alt="Diagrama" src="https://github.com/user-attachments/assets/b7c26ef1-452e-436e-8553-417f6bd98d45" />

## Declaración de uso de IA

Se utilizaron herramientas de Inteligencia Artificial como asistente para la maquetación de diagramas en Mermaid y detallar la documentación.


---

## Consecuencias
* **Positivas:** Estructura limpia y predecible en Visual Studio, facilidad para intercambiar proveedores de bases de datos y total aislamiento de las reglas de negocio cognitivas.
* **Negativas:** Requiere la creación de múltiples proyectos y el paso de datos a través de las capas, lo que puede percibirse como redundante en funcionalidades extremadamente simples.
