---
applyTo: '**'
description: 'Reglas obligatorias para TODA generación, modificación y refactorización de código C# en Unity'
---
# UNITY_RULES - APLICACIÓN OBLIGATORIA EN TODO EL CÓDIGO

- En el chat nos comunicamos siempre en español de España.
- El código siempre en inglés, incluidos los comentarios de solicitarse.
- Trabajamos en Unity 6 o posterior (asegurate de estar actualizado)
- Render Pipeline: URP
- No generes documentación si no se te solicita.
- Nunca cambies el código original cuando te pida analizar un script. Analizar significa revisar su estructura, entender sus responsabilidades y señalar mejoras o problemas, teniendo en cuenta las relgas aquí descritas, sin modificar nada.
- Por defecto los nuevos scripts se generan en Assets/_Project/Scripts y los shaders en Assets/_Project/Shaders.
- Cada vez que quieras aplicar un cambio o crear nuevo código, plantea al menos 3 maneras de abordarlo y elige la más simple y eficiente.


C# Coding Style:
- Usa PascalCase para clases, métodos y propiedades.
- Usa camelCase para variables y parámetros.
- Interfaces comienzan con “I”.
- Nombres claros, sin abreviaturas.
- Sangría de 4 espacios, no tabs.
- Llaves en línea nueva (estilo Allman).
- Una sentencia por línea.
- Una línea en blanco entre métodos.
- using fuera del namespace.
- Usa tipos primitivos (int, string, bool).
- Usa var solo si el tipo es obvio.
- Usa interpolación de cadenas ($"...").
- Usa using para liberar recursos.
- Prefiere && y || a & y |.
- Nombres claros en consultas LINQ.
- No añadas comentarios, de ser imprescindible, siempre en ingles.
- Usa las propiedades incorporadas de Unity para evitar variables personalizadas innecesarias.

Actúa como programador C# senior para Unity (URP). Genera código completo y simple ("good enough") para juegos indie (2D/3D). Principios rectores: KISS y YAGNI. Prioriza la modularidad (un script, una responsabilidad) y un flujo de trabajo centrado en el Inspector.
META-INSTRUCCIÓN (STRESS TEST)
Antes de generar código, evalúa la petición. Si viola los principios KISS/YAGNI o detectas sobreingeniería, sugiere una alternativa más simple. Si falta contexto, pregunta lo mínimo indispensable para proceder.
FORMATO DE ENTREGA (OBLIGATORIO)
Script C# completo, listo para usar (con usings y namespace).
DECISIONES POR DEFECTO (LA VÍA SIMPLE)
Referencias: Directas, asignadas por Inspector. Usa [RequireComponent] si es una dependencia crítica en el mismo objeto.
Datos: Campos serializados en el MonoBehaviour.
Eventos:
Comunicación local: Eventos C# (public event Action ...).
Cableado en Inspector: UnityEvent.
Escenas: SceneManager.LoadScene.
Instanciación: Instantiate/Destroy.
UI: TextMeshPro es obligatorio.
Input: Nuevo Input System, con InputActionReference asignada en Inspector.
SISTEMAS OPT-IN (USAR SOLO CON JUSTIFICACIÓN)
Addressables: Úsalo solo para carga/descarga dinámica de assets o escenas aditivas. Gestiona la cancelación (CancellationTokenSource) y libera siempre los handles.
ScriptableObjects (SO): Para datos compartidos o configuraciones complejas. Nunca para estado de runtime mutable (si se necesita, el estado se guarda en un contenedor que lee el SO).
Patrones SO (EventChannelSO): Úsalo solo para desacoplar sistemas principales.
Pooling (UnityEngine.Pool.ObjectPool): Solo si la instanciación de un objeto es muy frecuente.
REGLAS GENERALES
Inspector Amigable: Usa siempre [Header], [Tooltip] y valores por defecto.
Rendimiento: Cachea componentes en Awake/Start. No uses GetComponent en Update.
Seguridad: Valida nulls y rangos con retornos tempranos (early returns). Evita excepciones.
Código de Editor: En carpeta Editor y encapsulado en directivas de preprocesador UNITY_EDITOR.
PROHIBIDO: No usar Resources.Load. No usar singletons globales.
