# Arquitectura Técnica - Gallinas Felices

## 🏗️ Diagrama de Dependencias

```
TimeController (Singleton)
    ↓ eventos
Chicken ←→ ChickenEggProducer
    ↓              ↓
ChickenNeeds   Nest → Egg
    ↓              ↓
ChickenHappiness   EggCollector → EggCounter (Singleton)
                                       ↓
                                   EggCounterUI
                                       ↓
                                   BuyChickenButton → FarmManager
```

---

## 📦 Sistemas Modulares

### 1. Sistema de Tiempo (Core Service)
**Responsabilidad**: Controlar ciclo día/noche y emitir eventos temporales.

**Clase**: `TimeController` (Singleton)
- ✅ Un solo reloj para todo el juego
- ✅ Emite eventos `OnTimeOfDayChanged` y `OnHourChanged`
- ✅ Controla iluminación automáticamente
- ✅ Otros sistemas se suscriben sin acoplamiento

**Escalabilidad**:
- Puede añadir eventos estacionales (primavera, verano)
- Puede controlar clima/temperatura
- Puede disparar eventos especiales por hora específica

---

### 2. Sistema de Gallinas (Core Gameplay)

#### **Chicken.cs** - FSM Principal
**Responsabilidad**: Gestionar comportamiento y estado de la gallina.

**Estados FSM**:
```
Idle → Walking → Idle
  ↓      ↓        ↓
Eating  Drinking  Exploring
  ↓      ↓        ↓
LayingEgg ← Sleeping
```

**Transiciones basadas en**:
- Necesidades (hambre, sed, cansancio)
- Hora del día (dormir de noche)
- Timers de estado
- Personalidad (modificadores)

**Escalabilidad**:
- Añadir estados nuevos = añadir caso en switch
- FSM puede migrar a Behavior Tree manteniendo API
- Puede conectarse a sistemas de animación via `OnStateChanged`

#### **ChickenNeeds.cs** - Sistema de Necesidades
**Responsabilidad**: Trackear hambre/sed/energía.

**Datos**:
- `Hunger` (0-100)
- `Thirst` (0-100)
- `Energy` (0-100)

**Métodos públicos**:
- `IsHungry()`, `IsThirsty()`, `IsTired()`
- `Feed()`, `GiveWater()`, `RestoreEnergy()`

**Escalabilidad**:
- Añadir `Temperature`, `Hygiene`, etc.
- Sistema de buffs/debuffs temporal
- Integración con items consumibles

#### **ChickenHappiness.cs** - Sistema de Felicidad
**Responsabilidad**: Calcular felicidad basada en múltiples factores.

**Factores**:
- ✅ Comida disponible (+15%)
- ✅ Agua disponible (+15%)
- ✅ Sombra (+10%)
- ✅ Descansada (+10%)
- 🔄 Decoraciones cercanas (futuro)
- 🔄 Interacción con jugador (futuro)

**Impacto**:
- Modifica velocidad de producción de huevos (0.5x - 1.5x)
- Puede modificar comportamiento (más exploración si feliz)

**Escalabilidad**:
- Sistema de pesos dinámico
- Eventos de felicidad extrema (muy triste → no pone huevos)
- Buffs temporales

#### **ChickenEggProducer.cs** - Producción de Huevos
**Responsabilidad**: Gestionar timer y spawneo de huevos.

**Flujo**:
1. Timer cuenta atrás (modificado por felicidad + personalidad)
2. Al llegar a 0 → busca Nest disponible
3. Ocupa Nest → cambia estado Chicken a `LayingEgg`
4. Al finalizar estado → spawna Egg en Nest
5. Reset timer

**Escalabilidad**:
- Huevos de diferentes tipos (dorados, especiales)
- Sistema de críticos (doble producción)
- Eventos de boost (producción x2 por tiempo limitado)

---

### 3. Sistema de Estructuras (World Objects)

#### **ConsumableStructure.cs** - Clase Base
**Responsabilidad**: Base abstracta para Feeder/WaterTrough.

**Datos**:
- `maxCapacity`
- `currentCapacity`
- `consumptionAmount`

**Eventos**:
- `OnCapacityChanged`
- `OnEmpty`
- `OnRefilled`

**Escalabilidad**:
- Sistema de upgrades (aumentar `maxCapacity`)
- Diferentes tipos de comida/agua
- Auto-refill comprable
- Efectos visuales por nivel de llenado

#### **Nest.cs** - Punto de Producción
**Responsabilidad**: Gestionar ocupación y spawneo de huevos.

**Estados**:
- Libre → Ocupada (gallina poniendo) → Con huevo → Recogido → Libre

**Escalabilidad**:
- Nidos de calidad (mejoran huevos)
- Nidos especiales (huevos dorados)
- Upgrade de capacidad (múltiples huevos)

#### **Coop.cs** - Sistema de Dormitorios
**Responsabilidad**: Asignar slots de sueño a gallinas.

**Gestión**:
- Dictionary<Chicken, Transform> para asignaciones
- Slots configurables por Inspector
- Sistema de liberación automática

**Escalabilidad**:
- Coops de diferentes capacidades
- Bonus por dormir en coop (más felicidad)
- Decoraciones internas

#### **Egg.cs** - Recurso Coleccionable
**Responsabilidad**: Objeto que el jugador recoge.

**Interacción**:
- `OnMouseDown()` → llama `Collect()`
- Emite evento `OnCollected(value)`
- Se auto-destruye

**Escalabilidad**:
- Sistema de touch para mobile
- Animación de recogida
- VFX/SFX
- Tipos de huevos con valores diferentes

---

### 4. Sistema de Economía

#### **EggCounter.cs** - Moneda Global
**Responsabilidad**: Singleton que gestiona el total de huevos.

**API Pública**:
```csharp
void AddEggs(int amount)
bool TrySpendEggs(int amount)
bool CanAfford(int cost)
```

**Escalabilidad**:
- Múltiples monedas (huevos normales, dorados, especiales)
- Sistema de conversión
- Historial de transacciones
- Save/Load integration

#### **EggCollector.cs** - Puente Eggs → Counter
**Responsabilidad**: Conectar eventos de recogida con el contador.

**Patrón**: Observer pasivo que escucha a todos los Eggs.

**Escalabilidad**:
- Auto-recolector comprable (modo idle puro)
- Multiplicadores temporales
- Combo system (recoger X seguidos → bonus)

---

### 5. Sistema de UI

#### **EggCounterUI.cs** - Display Principal
**Responsabilidad**: Mostrar huevos en pantalla.

**Patrón**: Observer del EggCounter.

**Escalabilidad**:
- Animaciones de incremento
- Efecto de "juicy" feedback
- Diferentes formatos (K, M para números grandes)

#### **ChickenHappinessUI.cs** - UI Individual
**Responsabilidad**: Mostrar felicidad sobre cada gallina.

**Tipo**: World Space Canvas que sigue a la gallina.

**Escalabilidad**:
- Sistema de iconos (estados, necesidades)
- Tooltip con info detallada
- Oclusión inteligente

#### **BuyChickenButton.cs** - Compra Simple
**Responsabilidad**: Botón que gasta huevos y spawna gallina.

**Escalabilidad**:
- Tienda completa con múltiples items
- Sistema de unlocks
- Preview de lo que se compra
- Confirmación para compras caras

---

## 🎯 Patrones de Diseño Aplicados

### Singleton (Servicios Core)
**Dónde**: `TimeController`, `EggCounter`  
**Por qué**: Un solo reloj/contador global para todo el juego.  
**Cuidado**: Solo para servicios stateless o con estado global inevitable.

### State Machine (FSM)
**Dónde**: `Chicken.cs`  
**Por qué**: Comportamiento complejo con estados claros.  
**Implementación**: Enum + Switch (simple y debuggeable).  
**Migración**: Puede evolucionar a Behavior Tree sin cambiar API.

### Observer (Eventos)
**Dónde**: Todos los `UnityEvent` y C# `event Action`  
**Por qué**: Desacoplar sistemas (Egg → EggCollector → EggCounter).  
**Ventaja**: Inspector-friendly con UnityEvents.

### Template Method (Herencia)
**Dónde**: `ConsumableStructure` → `Feeder`, `WaterTrough`  
**Por qué**: Reutilizar lógica de capacidad/consumo.  
**Extensión**: Nuevos consumibles = heredar base.

### Strategy (ScriptableObjects)
**Dónde**: `ChickenPersonalitySO`, `GameBalanceSO`  
**Por qué**: Datos externos modificables sin código.  
**Ventaja**: Diseñadores pueden crear personalidades sin programar.

---

## 🔌 Puntos de Extensión Preparados

### 1. Animaciones
```csharp
// En Chicken.cs
private void ChangeState(ChickenState newState)
{
    CurrentState = newState;
    OnStateChanged?.Invoke(newState); // ← Conectar Animator aquí
}
```

**Implementación futura**:
```csharp
public class ChickenAnimator : MonoBehaviour
{
    [SerializeField] private Chicken chicken;
    [SerializeField] private Animator animator;

    void Start()
    {
        chicken.OnStateChanged.AddListener(OnStateChanged);
    }

    void OnStateChanged(ChickenState state)
    {
        animator.SetInteger("State", (int)state);
    }
}
```

### 2. VFX/SFX
```csharp
// En ChickenEggProducer.cs
OnEggProduced?.Invoke(); // ← Conectar partículas/sonido aquí
```

### 3. Mejoras de Estructuras
```csharp
// ConsumableStructure ya tiene:
public virtual void Upgrade(float capacityIncrease)
{
    maxCapacity += capacityIncrease;
    // Futuro: cambiar modelo 3D, VFX, etc.
}
```

### 4. Sistema de Guardado
```csharp
// En Chicken.cs - añadir:
[Serializable]
public class ChickenSaveData
{
    public string chickenName;
    public ChickenPersonalityType personality;
    public float hunger, thirst, energy;
    public float happiness;
    public Vector3 position;
}

public ChickenSaveData GetSaveData() { ... }
public void LoadSaveData(ChickenSaveData data) { ... }
```

### 5. Decoraciones/Buffs
```csharp
// Futuro sistema:
public class DecorationArea : MonoBehaviour
{
    [SerializeField] private float happinessBonus = 10f;

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Chicken>(out var chicken))
        {
            chicken.Happiness.AddTemporaryBonus("decoration", happinessBonus);
        }
    }
}
```

---

## 📊 Flujo de Datos Críticos

### Producción de Huevos (Ciclo Completo)
```
1. ChickenEggProducer.Update()
   ↓ timer -= deltaTime * multiplier
2. Timer <= 0 → TryProduceEgg()
   ↓
3. FindAvailableNest() → Physics.OverlapSphere
   ↓
4. Nest.TryOccupy() → isOccupied = true
   ↓
5. Chicken.StartLayingEgg() → FSM cambia estado
   ↓ OnStateChanged
6. ChickenEggProducer.OnChickenStateChanged()
   ↓
7. Nest.SpawnEgg() → Instantiate
   ↓
8. Egg creado, Nest.Release()
   ↓
9. Usuario clic → Egg.Collect()
   ↓ OnCollected(value)
10. EggCollector.OnEggCollected()
    ↓
11. EggCounter.AddEggs(value)
    ↓ OnEggCountChanged
12. EggCounterUI.UpdateDisplay()
```

### Ciclo Día/Noche
```
1. TimeController.Update()
   ↓ CurrentHour avanza
2. TimeOfDay cambia → OnTimeOfDayChanged.Invoke()
   ↓
3. Todas las Chickens reciben evento
   ↓
4. Si Night + IsTired() → GoToSleep()
   ↓
5. FSM cambia a Sleeping
   ↓
6. En Update: RestoreEnergy()
   ↓
7. Al amanecer → vuelve a Idle
```

---

## ⚡ Optimizaciones Futuras

### Object Pooling
**Cuándo**: Cuando tengas 50+ gallinas spawneando muchos huevos.
```csharp
using UnityEngine.Pool;

public class EggPool : MonoBehaviour
{
    private ObjectPool<Egg> pool;

    void Awake()
    {
        pool = new ObjectPool<Egg>(
            createFunc: () => Instantiate(eggPrefab),
            actionOnGet: (egg) => egg.gameObject.SetActive(true),
            actionOnRelease: (egg) => egg.gameObject.SetActive(false),
            actionOnDestroy: (egg) => Destroy(egg.gameObject),
            defaultCapacity: 20,
            maxSize: 100
        );
    }
}
```

### Spatial Partitioning
**Cuándo**: FindAvailableNest() es lento con 100+ nidos.
**Solución**: Registrar nidos en grid o QuadTree.

### Async Operations
**Cuándo**: NavMesh.SamplePosition() en muchas gallinas causa lag.
**Solución**: Usar Jobs System para pathfinding.

---

## 🧪 Testing Strategy

### Unit Tests (Post-MVP)
```csharp
[Test]
public void ChickenHappiness_CalculatesCorrectMultiplier()
{
    var happiness = new ChickenHappiness();
    happiness.SetHappiness(100f);
    Assert.AreEqual(1.5f, happiness.GetProductionMultiplier());
}

[Test]
public void EggCounter_CannotSpendMoreThanAvailable()
{
    var counter = new EggCounter();
    counter.AddEggs(10);
    Assert.IsFalse(counter.TrySpendEggs(20));
    Assert.AreEqual(10, counter.TotalEggs);
}
```

### Integration Tests
- Spawn gallina → verificar que produce huevo en X segundos
- Recoger 100 huevos → comprar gallina → verificar spawn
- Ciclo completo día → verificar gallinas duermen

---

## 📚 Referencias de Código

### Ejemplo: Conectar Animaciones
```csharp
// ChickenAnimationController.cs
public class ChickenAnimationController : MonoBehaviour
{
    [SerializeField] private Chicken chicken;
    [SerializeField] private Animator animator;
    
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    
    void Start()
    {
        chicken.OnStateChanged.AddListener(OnStateChanged);
    }
    
    void Update()
    {
        if (chicken.CurrentState == ChickenState.Walking)
        {
            animator.SetFloat(SpeedHash, agent.velocity.magnitude);
        }
    }
    
    void OnStateChanged(ChickenState state)
    {
        animator.SetInteger(StateHash, (int)state);
    }
}
```

### Ejemplo: Sistema de Mejoras
```csharp
// StructureUpgradeSystem.cs
[Serializable]
public class UpgradeData
{
    public int level;
    public int cost;
    public float capacityIncrease;
    public GameObject visualPrefab;
}

public class StructureUpgradeSystem : MonoBehaviour
{
    [SerializeField] private ConsumableStructure structure;
    [SerializeField] private List<UpgradeData> upgrades;
    
    private int currentLevel = 0;
    
    public bool TryUpgrade()
    {
        if (currentLevel >= upgrades.Count)
            return false;
            
        var upgrade = upgrades[currentLevel];
        
        if (EggCounter.Instance.TrySpendEggs(upgrade.cost))
        {
            structure.Upgrade(upgrade.capacityIncrease);
            // Cambiar visual, VFX, etc.
            currentLevel++;
            return true;
        }
        
        return false;
    }
}
```

---

**Documento actualizado**: Noviembre 2025  
**Versión**: MVP 1.0  
**Siguiente revisión**: Post-implementación de animaciones y VFX
