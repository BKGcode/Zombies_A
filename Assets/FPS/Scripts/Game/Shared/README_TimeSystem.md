# Sistema de Día/Noche - Documentación

## 📋 Resumen

Sistema completo de ciclo día/noche para juegos FPS con las siguientes características:

- ⏰ **Ciclo de tiempo configurable**: Día y noche con duración personalizable
- 🌅 **Iluminación dinámica**: Sol/luna con arco realista, skybox y luz ambiente
- 🎮 **Integración con gameplay**: Eventos horarios, cambios de comportamiento
- ⏸️ **Sistema de pausa**: Tiempo se pausa con menús/juego
- 🖥️ **UI informativa**: Reloj que muestra hora del juego

## 🏗️ Arquitectura

### Componentes Principales

#### 1. **DayNightCycle** (ScriptableObject)
- **Ubicación**: `Assets/FPS/Scripts/Game/Shared/DayNightCycle.cs`
- **Propósito**: Configuración centralizada del ciclo día/noche
- **Características**:
  - Duración del ciclo (día + noche)
  - Colores y rotación del sol/luna
  - Intensidad de luces
  - Horas de eventos específicos

#### 2. **TimeManager** (MonoBehaviour)
- **Ubicación**: `Assets/FPS/Scripts/Game/Shared/TimeManager.cs`
- **Propósito**: Control central del flujo del tiempo
- **Características**:
  - Singleton para acceso global
  - Gestión de pausa del juego
  - Eventos de cambio día/noche y horas específicas
  - Conversión tiempo real ↔ tiempo juego

#### 3. **LightingController** (MonoBehaviour)
- **Ubicación**: `Assets/FPS/Scripts/Game/Shared/LightingController.cs`
- **Propósito**: Control visual del ciclo día/noche
- **Características**:
  - Skybox dinámico
  - Luz direccional (sol/luna) con arco realista
  - Luz ambiente adaptativa
  - Transiciones suaves

#### 4. **TimeEventManager** (MonoBehaviour)
- **Ubicación**: `Assets/FPS/Scripts/Game/Shared/TimeEventManager.cs`
- **Propósito**: Gestión de eventos horarios
- **Características**:
  - Múltiples canales de eventos
  - Eventos día/noche y horas específicas
  - Gestión automática de estados

## 🚀 Configuración Básica

### 1. Crear el DayNightCycle

1. **Crear Asset**:
   - Project window → Right Click → Create → FPS/Game/Day Night Cycle
   - Nómbralo `DefaultDayNightCycle`

2. **Configurar parámetros**:
   ```csharp
   // 2 horas reales = 24 horas juego
   cycleDurationSeconds = 7200f; // 2 horas
   dayPercentage = 0.5f;         // 12 horas día
   nightPercentage = 0.5f;       // 12 horas noche

   // Eventos importantes
   eventHours = new int[] { 6, 12, 18, 0 }; // Amanecer, mediodía, atardecer, medianoche
   ```

### 2. Configurar TimeManager

1. **Crear GameObject**:
   - Scene → Create Empty → Nombre: `TimeManager`
   - Add Component: `TimeManager`

2. **Asignar configuración**:
   - Arrastra `DefaultDayNightCycle` al campo `Day Night Config`

### 3. Configurar LightingController

1. **Crear GameObject**:
   - Scene → Create Empty → Nombre: `LightingController`
   - Add Component: `LightingController`

2. **Asignar referencias**:
   - `Skybox Material`: Material del skybox de la escena
   - `Directional Light`: Luz direccional principal (sol)
   - `Day Night Config`: Mismo que TimeManager

3. **Añadir TimeManager**:
   - Add Component: `TimeManager` (al mismo GameObject)

## 📱 UI del Reloj

### Crear GameClockUI

1. **Crear Canvas hijo de UI**:
   - UI → Text - TextMeshPro

2. **Configurar componente**:
   ```csharp
   // Add Component: GameClockUI
   // Asignar el TextMeshPro al campo Clock Text
   clockFormat = ClockFormat.Format24H; // o Format12H
   ```

## 🎮 Eventos y Gameplay

### Crear eventos personalizados

1. **Crear canales de eventos**:
   ```csharp
   // Crear HourEventChannel para eventos específicos
   targetHour = 6;  // 6:00 AM - Amanecer
   oneTimePerCycle = true;

   // Crear DayNightEventChannel para cambios día/noche
   ```

2. **Crear controlador de eventos**:
   ```csharp
   public class MyTimeEvents : MonoBehaviour
   {
       [SerializeField] private HourEventChannel amanecerEvent;
       [SerializeField] private DayNightEventChannel diaNocheEvent;

       private void Start()
       {
           amanecerEvent.OnHourReached += OnAmanecer;
           diaNocheEvent.OnDayNightChanged += OnDiaNocheCambio;
       }

       private void OnAmanecer(int hora)
       {
           // Lógica para amanecer
           Debug.Log("¡Buenos días! El sol sale");
       }

       private void OnDiaNocheCambio(bool esDia)
       {
           if (esDia)
               Debug.Log("Es de día - Comportamiento diurno");
           else
               Debug.Log("Es de noche - Comportamiento nocturno");
       }
   }
   ```

## ⚙️ Configuración Avanzada

### Pausa del Sistema

```csharp
// Desde cualquier script
TimeManager.Instance.SetPaused(true);  // Pausar tiempo
TimeManager.Instance.SetPaused(false); // Reanudar tiempo
```

### Control Manual del Tiempo

```csharp
// Establecer hora específica
TimeManager.Instance.SetGameHour(15.5f); // 3:30 PM

// Avanzar tiempo
TimeManager.Instance.AdvanceTime(2f); // Avanzar 2 horas

// Consultar estado
bool esDia = TimeManager.Instance.IsDay();
float horaActual = TimeManager.Instance.GetCurrentGameHour();
```

### Ejemplo de Enemigos Dinámicos

```csharp
public class EnemyAI : MonoBehaviour
{
    private void Start()
    {
        TimeManager.Instance.OnDayNightChanged += OnDiaNocheCambio;
    }

    private void OnDiaNocheCambio(bool esDia)
    {
        if (esDia)
        {
            velocidadMovimiento = velocidadMovimiento * 0.8f; // Más lentos de día
            agresividad = agresividad * 0.7f; // Menos agresivos de día
        }
        else
        {
            velocidadMovimiento = velocidadMovimiento * 1.5f; // Más rápidos de noche
            agresividad = agresividad * 1.3f; // Más agresivos de noche
        }
    }
}
```

## 🐛 Debugging y Testing

### Comandos útiles para testing:

```csharp
// En Play Mode desde código
TimeManager.Instance.SetGameHour(6f);   // Saltar a amanecer
TimeManager.Instance.SetGameHour(18f);  // Saltar a atardecer
TimeManager.Instance.SetPaused(true);   // Pausar para observar
```

### Logs informativos:
- El sistema genera logs cuando cambian los períodos día/noche
- Eventos horarios se registran en consola
- Estados de transición se pueden monitorear

## 🎯 Próximos Pasos

1. **Sistema de fatiga del jugador** (como mencionaste)
2. **Comportamientos específicos de NPCs por hora**
3. **Eventos especiales (cambio de turno, emergencias)**
4. **Persistencia del tiempo entre sesiones**
5. **Skybox procedural más avanzado**

## ⚠️ Notas Importantes

- **Singleton**: TimeManager es singleton, solo una instancia por escena
- **DontDestroyOnLoad**: Persiste entre cambios de escena
- **Pausa automática**: Se conecta con Unity Time.timeScale
- **KISS**: Cada componente tiene una responsabilidad clara
- **Modular**: Fácil de extender con nuevos tipos de eventos

¡El sistema está listo para usar! 🚀
