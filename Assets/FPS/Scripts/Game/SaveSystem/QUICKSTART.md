# Sistema de Guardado - Guía Visual Rápida

## 📦 Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    SISTEMA DE GUARDADO                       │
└─────────────────────────────────────────────────────────────┘

┌──────────────────┐
│   SaveSystem     │ (Static - Sistema de Archivos)
│    [Static]      │ • SaveGame(slotName, data)
└────────┬─────────┘ • LoadGame(slotName)
         │           • DeleteSave(slotName)
         │           • SaveExists(slotName)
         ▼
    [JSON Files]
    slot1.json
    slot2.json
    autosave.json

┌──────────────────┐
│  GameData        │ (POCO - Datos Serializables)
│  [Serializable]  │ • playerHealth, playerPosition
└────────┬─────────┘ • unlockedWeapons, enemiesKilled
         │           • saveName, saveDate, totalPlayTime
         │
         ▼
┌──────────────────────┐
│ SaveSlotManager      │ (Manager Principal)
│ [MonoBehaviour]      │ • CurrentGameData
└────────┬─────────────┘ • SaveToSlot(slotName)
         │               • LoadFromSlot(slotName)
         │               • NewGame()
         │               • Events: OnGameSaved, OnGameLoaded
         │
         ├─────────────────────┐
         ▼                     ▼
┌──────────────────┐  ┌──────────────────┐
│SaveDataCollector │  │  ISaveable       │ (Interface)
│[MonoBehaviour]   │  │  [Interface]     │ • SaveData(GameData)
└────────┬─────────┘  └────────┬─────────┘ • LoadData(GameData)
         │                     │
         │                     │
         └─────────┬───────────┘
                   │
         ┌─────────┴─────────┐
         ▼                   ▼
┌──────────────────┐  ┌──────────────────────┐
│ HealthSaveable   │  │PlayerTransformSaveable│
│[MonoBehaviour]   │  │   [MonoBehaviour]     │
└──────────────────┘  └───────────────────────┘
```

## 🎯 Flujo de Datos

### GUARDAR:
```
Jugador presiona "Guardar"
        │
        ▼
[UI] SaveLoadUIExample.SaveToSlot(1)
        │
        ▼
[Manager] SaveSlotManager.SaveToSlot(1)
        │
        ├─▶ [Auto] SaveDataCollector.CollectAllData()
        │           │
        │           ├─▶ HealthSaveable.SaveData(gameData)
        │           └─▶ PlayerTransformSaveable.SaveData(gameData)
        │
        ▼
[System] SaveSystem.SaveGame("slot1", gameData)
        │
        ▼
[File] slot1.json guardado en disco
        │
        ▼
[Event] OnGameSaved.Invoke("slot1")
        │
        ▼
[UI] Muestra "Partida guardada!"
```

### CARGAR:
```
Jugador selecciona Slot 1
        │
        ▼
[UI] SaveLoadUIExample.LoadFromSlot(1)
        │
        ▼
[Manager] SaveSlotManager.LoadFromSlot(1)
        │
        ▼
[System] SaveSystem.LoadGame("slot1")
        │
        ▼
[File] slot1.json leído desde disco
        │
        ▼
[Manager] CurrentGameData = loadedData
        │
        ▼
[Event] OnGameLoaded.Invoke(gameData)
        │
        ├─▶ [Auto] SaveDataCollector.ApplyAllData()
        │           │
        │           ├─▶ HealthSaveable.LoadData(gameData)
        │           └─▶ PlayerTransformSaveable.LoadData(gameData)
        │
        ▼
[UI] Muestra "Partida cargada!"
```

## 🔧 Setup en Unity Editor

### 1. GameObject Hierarchy
```
Scene
├── SaveManager (GameObject vacío)
│   ├── SaveSlotManager (Component)
│   ├── SaveDataCollector (Component)
│   └── SaveSystemDebugger (Component) [Opcional]
│
├── Player
│   ├── Health (Component existente)
│   ├── HealthSaveable (Component nuevo) ✓
│   └── PlayerTransformSaveable (Component nuevo) ✓
│
└── UI
    └── SaveLoadMenu (Canvas)
        └── SaveLoadUIExample (Component)
            ├── Slot1 Panel
            ├── Slot2 Panel
            └── Slot3 Panel
```

### 2. Inspector - SaveSlotManager
```
┌─────────────────────────────────┐
│ Save Slot Manager (Script)      │
├─────────────────────────────────┤
│ Configuration                   │
│ Max Slots: 3                    │
│ Auto Save Slot Name: autosave   │
│ Auto Save Interval: 300         │
├─────────────────────────────────┤
│ Events                          │
│ On Game Saved ()                │
│ On Game Loaded ()               │
│ On Save Error ()                │
└─────────────────────────────────┘
```

### 3. Inspector - SaveDataCollector
```
┌─────────────────────────────────┐
│ Save Data Collector (Script)    │
├─────────────────────────────────┤
│ References                      │
│ Save Slot Manager: [Drag here]  │
├─────────────────────────────────┤
│ Configuration                   │
│ ☑ Auto Collect On Save          │
│ ☑ Auto Apply On Load            │
└─────────────────────────────────┘
```

## 🎮 Controles Rápidos (Debug)

### En Editor (Play Mode):
```
F5  = Quick Save (test_slot)
F9  = Quick Load (test_slot)
```

### Context Menu (Click derecho en componente):
```
SaveSystemDebugger
├── Quick Save (F5)
├── Quick Load (F9)
├── Delete Test Slot
├── List All Save Slots
├── Print Current GameData
└── New Game
```

### Tools Menu:
```
Tools/Save System/
├── Open Save Folder
└── Show Save Path
```

## 📝 Código Común

### Guardar desde código:
```csharp
// Referencia en Inspector
public SaveSlotManager saveSlotManager;

// Guardar en slot 1
saveSlotManager.SaveToSlot(1);

// Guardar en slot custom
saveSlotManager.SaveToSlot("my_custom_save");

// Autosave
saveSlotManager.AutoSave();
```

### Cargar desde código:
```csharp
// Cargar slot 1
saveSlotManager.LoadFromSlot(1);

// Verificar antes de cargar
if (saveSlotManager.SlotHasSave("slot1"))
{
    saveSlotManager.LoadFromSlot("slot1");
}
```

### Acceder a datos actuales:
```csharp
// Leer datos
int kills = saveSlotManager.CurrentGameData.enemiesKilled;
float health = saveSlotManager.CurrentGameData.playerHealth;

// Modificar datos
saveSlotManager.CurrentGameData.enemiesKilled++;
saveSlotManager.MarkAsModified(); // ¡Importante!
```

### Crear componente guardable:
```csharp
using Unity.FPS.Game;

public class MyComponent : MonoBehaviour, ISaveable
{
    public int myValue;

    public void SaveData(GameData data)
    {
        // Guardar en GameData existente
        data.currentWaveNumber = myValue;
    }

    public void LoadData(GameData data)
    {
        // Cargar desde GameData
        myValue = data.currentWaveNumber;
    }
}
```

## 📁 Estructura de Archivos

```
Assets/FPS/Scripts/Game/SaveSystem/
├── README.md                    ← Documentación completa
├── QUICKSTART.md                ← Esta guía
├── GameData.cs                  ← Datos serializables
├── SaveSystem.cs                ← Sistema de archivos
├── SaveSlotManager.cs           ← Manager principal
├── ISaveable.cs                 ← Interfaz
├── SaveDataCollector.cs         ← Recolector automático
├── HealthSaveable.cs            ← Ejemplo Health
├── PlayerTransformSaveable.cs   ← Ejemplo Transform
└── SaveSystemDebugger.cs        ← Debug tools

Assets/FPS/Scripts/UI/
└── SaveLoadUIExample.cs         ← Ejemplo UI
```

## ✅ Checklist Rápido

```
Setup Inicial:
☐ 1. Crear GameObject "SaveManager"
☐ 2. Añadir SaveSlotManager
☐ 3. Añadir SaveDataCollector
☐ 4. Conectar referencias

Hacer Jugador Guardable:
☐ 5. Añadir HealthSaveable al Player
☐ 6. Añadir PlayerTransformSaveable al Player
☐ 7. Verificar que Health y Transform existen

Test:
☐ 8. Play mode → F5 para guardar
☐ 9. Modificar salud/posición
☐ 10. F9 para cargar
☐ 11. Verificar que se restauró

Producción:
☐ 12. Crear UI de guardado/carga
☐ 13. Conectar eventos OnGameSaved/OnGameLoaded
☐ 14. Eliminar SaveSystemDebugger del build
```

## 🚀 Siguiente Paso

**¿Todo listo?** → Ve a `README.md` para documentación completa

**¿Problemas?** → Usa SaveSystemDebugger para diagnosticar

**¿Quieres más?** → Extiende GameData con tus propios campos
