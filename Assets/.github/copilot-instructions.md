# POWERchickens - AI Coding Guide

## Proyecto
Simulador de granja de gallinas contemplativo en Unity 6 (URP). Los jugadores gestionan gallinas con personalidades únicas, necesidades y producción de huevos. Economía basada en huevos, estructuras mejorables y ciclo día/noche dinámico. Filosofía: experiencia relajante, no presión.

## Arquitectura Core

### Namespace Organization
- `GallinasFelices.Core` - Managers principales (FarmManager, TimeController, InteractionManager, EggCounter, PanelManager, FarmAutoManager, FarmLimits)
- `GallinasFelices.Chicken` - Sistema de gallinas (Chicken, ChickenNeeds, ChickenHappiness, ChickenEggProducer, ChickenLifespan, ChickenIdleFidget)
- `GallinasFelices.Structures` - Estructuras de granja (Feeder, WaterTrough, Coop, Nest, ConsumableStructure base, StructureDurability)
- `GallinasFelices.Data` - ScriptableObjects de configuración (*SO.cs)
- `GallinasFelices.UI` - UI controllers (InteractionInfoPanel, EggCounterUI, StructureStatusUI)
- `GallinasFelices.VFX` - Efectos visuales (SpawnEffect, ClickFeedback, ChickenWarningIcon, ChickenSleepBreathingAnimation)
- `HappyChickens.Debug` - Herramientas de debug (ChickenDebugger, ChickenMonitorManager) - solo `#if UNITY_EDITOR`

### Flujo de Datos ScriptableObject-First
**CRÍTICO**: Toda configuración balanceable vive en ScriptableObjects, NO en hardcoded values:
- `GameBalanceSO` - Balance global (costos, tiempos, consumptionPerUse, ciclo día/noche)
- `ChickenPersonalitySO` - Comportamientos (walkSpeedMultiplier, idle timings, egg production, avoidancePriority, fidget params)
- `ChickenConfigSO` - Datos visuales y nombres aleatorios
- `FeederConfigSO`/`CoopConfigSO`/`WaterTroughConfigSO` - Capacidades, usuarios simultáneos, mejoras con nextLevel chaining
- `UITextsConfigSO` - Todos los strings de UI localizables
- `FarmLimitsSO` - Límites de población

**Pattern**: MonoBehaviours serializan referencias a SOs y leen valores en runtime. El estado mutable vive en MonoBehaviours, la configuración inmutable en SOs. Los upgrades encadenan SOs (`nextLevel` reference).

```csharp
// ✅ CORRECTO
[SerializeField] private GameBalanceSO gameBalance;
float cost = gameBalance.feederRefillCost;

// ❌ MAL
private const int REFILL_COST = 10; // Hardcoded
```

### Sistema de Máquina de Estados (Chicken.cs)
Las gallinas usan una FSM modular con estados: `Idle`, `Walking`, `Eating`, `Drinking`, `Sleeping`, `GoingToEat`, `GoingToDrink`, `GoingToSleep`, `GoingToNest`, `LayingEgg`, `Exploring`.

**Flujo clave**: `ShouldHandleNeeds()` en `HandleIdleState()` evalúa prioridades (sueño > hambre > sed). Las gallinas usan NavMeshAgent con variaciones de velocidad y pausas aleatorias basadas en `ChickenPersonalitySO` (walkSpeedMultiplier, minIdleTime/maxIdleTime).

**Hallazgo de estructuras**: `FindAvailableFeeder()`, `FindAvailableCoop()`, etc. buscan la más cercana no rota y con capacidad. Verifican `StructureDurability.IsBroken` antes de asignar.

**Comportamiento natural**: `ChickenIdleFidget` añade rotaciones sutiles (maxFidgetAngle, fidgetRotationSpeed) para evitar apariencia robótica. Thresholds aleatorios por personalidad.

### Sistema de Interacción
`IInteractable` permite clickear objetos. `InteractionManager` (InputActionReference para click) hace raycast → `PanelManager` muestra `InteractionInfoPanel` con:
- `GetTitle()` - Nombre
- `GetMainInfo()` - Estado/warnings
- `GetBars()` - Barras de progreso (vida, felicidad, capacidad, durabilidad)
- `GetActions()` - Botones con costo en huevos y validación `CanAfford()`

Implementado por `Chicken`, `ConsumableStructure` (base para Feeder/WaterTrough/etc). `ClickFeedback` VFX auto-añadido en runtime con DOTween squash/stretch.

### Economía de Huevos
`EggCounter` (singleton) gestiona la moneda. `ChickenEggProducer` (componente separado) maneja layingEggProgress (0-100), spawn eggs cuando llega a 100. Eggs clickeables (`Egg.cs`) incrementan contador al ser recogidos.

Costos definidos en `GameBalanceSO`: `feederRefillCost`, `chickenPurchaseCost`, etc. Todas las estructuras usan `EggCounter.Instance.TrySpendEggs()` para transacciones.

### Ciclo Día/Noche
`TimeController` (singleton) avanza `CurrentHour` (0-24) basado en `GameBalanceSO.secondsPerGameHour`. Triggers `OnTimeOfDayChanged` (C# event) → gallinas reaccionan a `TimeOfDay.Night`. Actualiza lighting (sunLight intensity/rotation) y skybox blend si aplicable.

### Sistema de Estructuras
`ConsumableStructure` (abstract base) maneja:
- Capacidad (`currentCapacity`/`MaxCapacity` abstract property definida en SO por estructura hija)
- Durabilidad (`StructureDurability` componente, llama OnStructureUsed())
- Usuarios simultáneos (`currentUsers` vs `GetMaxSimultaneousUsers()` abstract)
- Acciones de UI (Refill, Repair, Upgrade si `CanUpgrade`)

**Pattern de upgrades**: SOs encadenan `nextLevel` (ej: FeederConfigSO Level 1 → Level 2). `Upgrade()` swapea el SO activo.

**Herencia**: `Feeder`, `WaterTrough`, `Coop`, `Nest` heredan de `ConsumableStructure` e implementan abstracts.

### Spawning & Visual System
`FarmManager` spawn gallinas con:
1. Instancia prefab base Chicken.prefab
2. Visual aleatorio: `chickenVisualPrefabs` instanciado como hijo (visualRoot)
3. `ChickenPersonalitySO` random asignado (afecta velocidad, idle, egg production)
4. `ChickenConfigSO` random asignado (nombres, textos de estado)
5. Auto-add `ChickenDebugger` en editor (`#if UNITY_EDITOR`)

**VFX**: `SpawnEffect` con DOTween para spawn bounce/squash. `ChickenSleepBreathingAnimation` durante sueño.

### Sistema de Auto-gestión
`FarmAutoManager` maneja:
- Auto-repair de estructuras rotas cuando hay huevos suficientes
- Auto-refill de estructuras vacías
- Activación/desactivación via Inspector toggles

`FarmLimits` controla:
- Límites de población (min/max desde `FarmLimitsSO`)
- Warnings de coop lleno (`CoopFullIndicator` VFX)
- Muerte de gallinas por abandono cuando excede límites

## Convenciones de Código
Sigue `.github/instructions/UnityRules.instructions.md`:
- **KISS/YAGNI**: Código "good enough", no overengineering
- **Inspector-first**: Todo serializado, usa `[Header]`, `[Tooltip]`, valores default
- **Input System**: Nuevo Input System con `InputActionReference` asignadas en Inspector (ver `CameraController.cs`, `InteractionManager.cs`)
- **TextMeshPro obligatorio** para UI
- **NavMesh**: Unity NavMeshComponents (carpeta `NavMeshComponents/`) para baking runtime
- **DOTween**: Para animaciones VFX (ClickFeedback, SpawnEffect, UISquashStretchLoop, breathing). DOTween Pro en `Plugins/Demigiant/DOTweenPro/`
- **NO singletons globales** excepto managers core (EggCounter, TimeController, FarmManager) con null-check en Awake
- **NO Resources.Load**
- **Cachea componentes** en Awake/Start, nunca GetComponent en Update
- **Modularidad**: Un script = una responsabilidad. Ej: Chicken.cs orquesta, ChickenNeeds/ChickenHappiness/ChickenEggProducer componentes separados

## Workflows de Desarrollo

### Adding a New Structure
1. Hereda de `ConsumableStructure` (abstract base en `Structures/`)
2. Crea SO de config (heredar de `ScriptableObject`, carpeta `Data/`)
3. Implementa abstract methods: `MaxCapacity` (property), `GetMaxSimultaneousUsers()`, optional overrides para GetStructureTitle(), GetRefillCost(), GetRepairCost()
4. OnTriggerEnter: verifica `IsEmpty`, `IsBroken`, `IsFull` → llama `TryStartUsing()`/`TryConsume()`
5. Añade prefab a `Assets/Prefabs/Structures/`
6. Implementa `IInteractable` si necesita UI específica (o usa base de ConsumableStructure)

### Adding a Chicken Behavior
1. Crea nuevo estado en `ChickenState` enum (`Chicken/ChickenState.cs`)
2. Añade `Handle{Estado}State()` method en `Chicken.cs`
3. Actualiza `UpdateStateMachine()` switch con el nuevo estado
4. Considera si necesita:
   - Target navigation: `SetDestination()` + `hasReachedDestination` check
   - Timer: `stateTimer` para duración de estado
   - Interaction con estructura: asigna `currentTarget` (Transform)
5. Actualiza `ChickenPersonalitySO` si necesita configuración específica (ej: min/maxTime)

### Debugging Chickens
- Editor: `FarmManager` auto-añade `ChickenDebugger` en `#if UNITY_EDITOR` (logs transiciones, needs, stuck detection)
- `ChickenMonitorManager` + `ChickenMonitorWindow` (Window > Happy Chickens > Monitor) rastrea todas las gallinas en runtime
- `ChickenDebugReport` genera informes de estado en consola
- Usa Gizmos: `OnDrawGizmosSelected()` muestra wander radius, NavMesh paths (implementado en Chicken.cs)
- Layer mask: Interactables en layer específico (configurable en InteractionManager)

### Creating ScriptableObjects
- Menu: Create > GallinasFelices > {tipo}
- Ubicación: `Assets/Scripts/ScriptableObjects/` para instances
- Naming: Descriptivo (ej: "Personality_Lazy", "GameBalance_Main")
- Chaining: Para upgrades usa `nextLevel` field (ej: FeederConfigSO Level 1 → Level 2)

## Integración con Assets Externos
- **FPS Microgame**: Presente en carpeta `FPS/` pero NO integrado al chicken system (ignorar)
- **Beautify/DynamicFogURP**: Post-processing en URP (carpetas `Beautify/URP/`, `DynamicFogURP/`)
- **NavMeshComponents**: Unity package en carpeta local, permite baking runtime NavMesh
- **DOTween**: Tweening library (Plugins/Demigiant/). DOTween Pro para TextMeshPro tweens
- **TextMesh Pro**: Carpeta `TextMesh Pro/` - ejemplos en `Examples & Extras/` (no tocar para el juego)
- **Layer Lab/LUT Pack**: UI/LUTs assets (no core gameplay)

## File Locations
- Scripts proyecto: `Assets/Scripts/` (organizado por namespace folders)
- Nuevos scripts: `Assets/Scripts/{namespace}/` (ej: `Scripts/Chicken/`, `Scripts/Core/`)
- ScriptableObjects definitions: `Assets/Scripts/Data/`
- ScriptableObject instances: `Assets/Scripts/ScriptableObjects/` (assets creados)
- Escenas: `Assets/Scenes/` (test scene: `SCN_ChickenTest_A.unity`)
- Prefabs: `Assets/Prefabs/` (Chicken/, Structures/, UI/, ICONS/)
- Editor tools: `Assets/Scripts/Editor/` (ChickenMonitorWindow, ChickenPrefabGenerator)
- Debug scripts: `Assets/Scripts/Debug/` (con `#if UNITY_EDITOR` guards)
- VFX: `Assets/Scripts/VFX/` (componentes visuales reutilizables)
- Art assets: `Assets/ART/` (meshes, materials, textures, prefabs visuales)

## Testing
No hay framework de tests unitarios. Testing manual en escena `SCN_ChickenTest_A.unity`:
- Play Mode testing con ChickenDebugger logs
- ChickenMonitorWindow para overview de todas las gallinas
- Scene View Gizmos para debug visual (paths, radios)
- Console logs con prefijos por sistema: `[Chicken]`, `[FarmManager]`, `[EggCounter]`

## Input System
Nuevo Input System (Unity InputActions):
- InputActions asset: `Assets/InputSystem.inputsettings.asset`
- Pattern: `InputActionReference` fields serializados en Inspector
- Enable/Disable en OnEnable/OnDisable del MonoBehaviour
- Ejemplo: `InteractionManager.cs` (click action), `CameraController.cs` (zoom/pan actions)
- NO usar Input.GetKey() o Input.GetButton() legacy

## Idioma
- **Comunicación**: Español de España
- **Código**: Inglés (clases, métodos, variables)
- **UI/Logs**: Español (via `UITextsConfigSO`)
- **Comentarios**: Solo si imprescindible, en inglés
- **GDD**: Documentación en español (`Assets/Scripts/Documentation/GDD.txt`)
