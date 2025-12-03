# POWERchickens - AI Coding Guide

## Proyecto
Simulador de granja de gallinas en Unity 6 (URP). Los jugadores gestionan gallinas con personalidades únicas, necesidades y producción de huevos. Economía basada en huevos, estructuras mejorables y ciclo día/noche dinámico.

## Arquitectura Core

### Namespace Organization
- `GallinasFelices.Core` - Managers principales (FarmManager, TimeController, InteractionManager, EggCounter)
- `GallinasFelices.Chicken` - Sistema de gallinas (Chicken, ChickenNeeds, ChickenHappiness, ChickenEggProducer, ChickenLifespan)
- `GallinasFelices.Structures` - Estructuras de granja (Feeder, WaterTrough, Coop, Nest, ConsumableStructure)
- `GallinasFelices.Data` - ScriptableObjects de configuración (*SO.cs)
- `GallinasFelices.UI` - UI controllers
- `HappyChickens.Debug` - Herramientas de debug (solo #if UNITY_EDITOR)

### Flujo de Datos ScriptableObject-First
**CRÍTICO**: Toda configuración balanceable vive en ScriptableObjects, NO en hardcoded values:
- `GameBalanceSO` - Balance global (costos, tiempos, ciclo día/noche)
- `ChickenPersonalitySO` - Comportamientos (velocidad, idle, producción de huevos)
- `ChickenConfigSO` - Datos visuales y nombres aleatorios
- `FeederConfigSO`/`CoopConfigSO`/`WaterTroughConfigSO` - Capacidades, usuarios simultáneos, mejoras
- `UITextsConfigSO` - Todos los strings de UI localizables

**Pattern**: MonoBehaviours serializan referencias a SOs y leen valores en runtime. El estado mutable vive en MonoBehaviours, la configuración inmutable en SOs.

```csharp
// ✅ CORRECTO
[SerializeField] private GameBalanceSO gameBalance;
float cost = gameBalance.feederRefillCost;

// ❌ MAL
private const int REFILL_COST = 10; // Hardcoded
```

### Sistema de Máquina de Estados (Chicken.cs)
Las gallinas usan una FSM simple con estados: `Idle`, `Walking`, `Eating`, `Drinking`, `Sleeping`, `GoingToEat`, `GoingToDrink`, `GoingToSleep`, `GoingToNest`, `LayingEgg`, `Exploring`.

**Flujo clave**: `ShouldHandleNeeds()` en `HandleIdleState()` evalúa prioridades (sueño > hambre > sed). Las gallinas usan NavMeshAgent con variaciones de velocidad y pausas aleatorias basadas en `ChickenPersonalitySO`.

**Hallazgo de estructuras**: `FindAvailableFeeder()`, `FindAvailableCoop()`, etc. buscan la más cercana no rota y con capacidad. Verifican `StructureDurability.IsBroken` antes de asignar.

### Sistema de Interacción
`IInteractable` permite clickear objetos. `InteractionManager` hace raycast de cámara → muestra `InteractionInfoPanel` con:
- `GetTitle()` - Nombre
- `GetMainInfo()` - Estado/warnings
- `GetBars()` - Barras de progreso (vida, felicidad, capacidad, durabilidad)
- `GetActions()` - Botones con costo en huevos y validación `CanAfford()`

Implementado por `Chicken`, `ConsumableStructure` (base para Feeder/WaterTrough), etc.

### Economía de Huevos
`EggCounter` (singleton) gestiona la moneda. `ChickenEggProducer` spawn eggs cuando `layingEggProgress >= 100`. Eggs clickeables (`Egg.cs`) incrementan contador al ser recogidos.

Costos definidos en `GameBalanceSO`: `feederRefillCost`, `chickenPurchaseCost`, etc. Todas las estructuras usan `TrySpendEggs()` para transacciones.

### Ciclo Día/Noche
`TimeController` avanza `CurrentHour` (0-24) basado en `GameBalanceSO.secondsPerGameHour`. Triggers `OnTimeOfDayChanged` → gallinas van a dormir en `TimeOfDay.Night`. Actualiza lighting (sunLight intensity/rotation) y skybox blend.

### Sistema de Estructuras
`ConsumableStructure` (abstract base) maneja:
- Capacidad (`currentCapacity`/`MaxCapacity` definida en SO)
- Durabilidad (`StructureDurability` componente)
- Usuarios simultáneos (`currentUsers` vs `GetMaxSimultaneousUsers()`)
- Acciones de UI (Rellenar, Reparar, Mejorar si `CanUpgrade`)

**Pattern de upgrades**: SOs encadenan `nextLevel` (ej: FeederConfigSO Level 1 → Level 2). `Upgrade()` swapea el SO activo.

### Spawning & Personality
`FarmManager` spawn gallinas con:
1. Visual aleatorio (`chickenVisualPrefabs`) como hijo
2. `ChickenPersonalitySO` random (afecta velocidad, idle, egg production)
3. `ChickenConfigSO` random (nombres, textos de estado)
4. Auto-add `ChickenDebugger` en editor

## Convenciones de Código
Sigue `.github/instructions/UnityRules.instructions.md`:
- **KISS/YAGNI**: Código "good enough", no overengineering
- **Inspector-first**: Todo serializado, usa `[Header]`, `[Tooltip]`, valores default
- **Input System**: `InputActionReference` asignadas en Inspector (ver `CameraController.cs`, `InteractionManager.cs`)
- **TextMeshPro obligatorio** para UI
- **NavMesh**: Usa Unity NavMeshComponents (en `NavMeshComponents/`)
- **NO singletons globales** (excepto managers core con null-check)
- **NO Resources.Load**
- **Cachea componentes** en Awake/Start, nunca GetComponent en Update

## Workflows de Desarrollo

### Adding a New Structure
1. Hereda de `ConsumableStructure` o crea nueva base
2. Crea SO de config (heredar de `ScriptableObject`)
3. Implementa abstract methods: `MaxCapacity`, `GetStructureTitle()`, `GetRefillCost()`, `GetRepairCost()`
4. OnTriggerEnter: verifica estado del chicken y llama `TryStartUsing()`/`TryConsume()`
5. Añade prefab a `Assets/Prefabs/Structures/`

### Adding a Chicken Behavior
1. Crea nuevo estado en `ChickenState` enum
2. Añade `Handle{Estado}State()` en `Chicken.cs`
3. Actualiza `UpdateStateMachine()` switch
4. Considera si necesita target navigation (`SetDestination()`) o timer (`stateTimer`)

### Debugging Chickens
- Editor: Auto-añade `ChickenDebugger` (logs transiciones, needs, stuck detection)
- `ChickenMonitorManager` rastrea todas las gallinas en runtime
- Usa Gizmos: `OnDrawGizmosSelected()` muestra wander radius, NavMesh paths

## Integración con Assets Externos
- **FPS Microgame**: Presente pero no integrado al chicken system (carpeta `FPS/`)
- **Beautify/DynamicFogURP**: Post-processing en URP
- **NavMeshComponents**: Baking runtime opcional
- **TextMesh Pro**: Ejemplos en `TextMesh Pro/Examples & Extras/` (ignorar para el juego)

## File Locations
- Scripts proyecto: `Assets/Scripts/`
- Nuevos scripts: `Assets/_Project/Scripts/` (aunque actualmente usa `Scripts/`)
- Shaders custom: `Assets/_Project/Shaders/` o `Assets/Shaders/`
- ScriptableObjects: `Assets/Scripts/ScriptableObjects/` (assets creados)
- Escenas: `Assets/Scenes/` (test: `SCN_ChickenTest_A.unity`)
- Prefabs: `Assets/Prefabs/` (Chicken/, Structures/, UI/)

## Testing
No hay framework de tests. Testing manual en `SCN_ChickenTest_A`. Usa Play Mode + ChickenDebugger para verificar FSM y needs.

## Idioma
- **Comunicación**: Español de España
- **Código**: Inglés (clases, métodos, variables)
- **UI/Logs**: Español (via `UITextsConfigSO`)
- **Comentarios**: Solo si imprescindible, en inglés
