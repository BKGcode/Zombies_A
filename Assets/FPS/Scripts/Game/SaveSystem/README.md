# Sistema de Guardado para Unity FPS Game

Sistema modular de guardado y carga de partidas con soporte para múltiples slots.

## 📁 Estructura

```
SaveSystem/
├── GameData.cs               # Datos serializables de la partida
├── SaveSystem.cs             # Sistema de archivos (static)
├── SaveSlotManager.cs        # Manager de slots (MonoBehaviour)
├── ISaveable.cs              # Interfaz para componentes guardables
├── SaveDataCollector.cs      # Recolector automático de datos
├── HealthSaveable.cs         # Ejemplo: guardar Health
├── PlayerTransformSaveable.cs # Ejemplo: guardar posición
└── SaveSystemDebugger.cs     # Herramientas de debug
```

## 🚀 Setup Rápido

### 1. Crear GameObject Manager
```
1. Crear GameObject vacío llamado "SaveManager"
2. Añadir componentes:
   - SaveSlotManager
   - SaveDataCollector
   - SaveSystemDebugger (opcional, solo para testing)
```

### 2. Configurar SaveSlotManager
```
Inspector:
- Max Slots: 3 (o los que necesites)
- Auto Save Slot Name: "autosave"
- Auto Save Interval: 300 (segundos, 0 = desactivado)
```

### 3. Configurar SaveDataCollector
```
Inspector:
- Save Slot Manager: Arrastra el SaveSlotManager
- Auto Collect On Save: ✓
- Auto Apply On Load: ✓
```

### 4. Hacer componentes guardables
```csharp
// En el GameObject del Player:
1. Añadir HealthSaveable (al mismo GameObject que tiene Health)
2. Añadir PlayerTransformSaveable (al Transform principal)
```

## 💾 Uso Básico

### Guardar Partida
```csharp
// Guardar en slot numerado
saveSlotManager.SaveToSlot(1); // slot1

// Guardar en slot custom
saveSlotManager.SaveToSlot("my_save");

// Autosave
saveSlotManager.AutoSave();
```

### Cargar Partida
```csharp
// Cargar desde slot
saveSlotManager.LoadFromSlot(1);

// Cargar custom
saveSlotManager.LoadFromSlot("my_save");
```

### Verificar/Eliminar
```csharp
// Verificar si existe
if (saveSlotManager.SlotHasSave("slot1"))
{
    // Existe guardado
}

// Eliminar slot
saveSlotManager.DeleteSlot("slot1");

// Preview sin cargar
GameData preview = saveSlotManager.PreviewSlot("slot1");
```

## 🎮 Eventos UnityEvent

Conecta en Inspector para reaccionar a guardado/carga:

```
SaveSlotManager:
- OnGameSaved(string slotName)    → Mostrar "Guardado!"
- OnGameLoaded(GameData data)     → Aplicar datos, cambiar escena
- OnSaveError(string error)       → Mostrar error UI
```

## 🔧 Crear Componentes Guardables

### Opción 1: Implementar ISaveable

```csharp
using Unity.FPS.Game;

public class MyComponent : MonoBehaviour, ISaveable
{
    public int myValue;

    public void SaveData(GameData data)
    {
        // Guardar en GameData
        data.enemiesKilled = myValue;
    }

    public void LoadData(GameData data)
    {
        // Cargar desde GameData
        myValue = data.enemiesKilled;
    }
}
```

### Opción 2: Extender GameData

```csharp
// En GameData.cs, añadir campos:
[Header("New Feature")]
public int newFeatureValue;
public string[] newFeatureArray;

// Luego usar desde cualquier script:
saveSlotManager.CurrentGameData.newFeatureValue = 123;
```

## 📊 GameData - Campos Disponibles

```csharp
// Información del guardado
string saveName           // Nombre de la partida
string saveDate           // Fecha/hora de guardado
float totalPlayTime       // Tiempo total jugado

// Jugador
float playerHealth        // Vida actual
float playerMaxHealth     // Vida máxima
Vector3 playerPosition    // Posición
Vector3 playerRotation    // Rotación

// Armas
string[] unlockedWeapons  // Armas desbloqueadas
int activeWeaponIndex     // Arma activa

// Progreso
int enemiesKilled         // Enemigos eliminados
int objectivesCompleted   // Objetivos completados
string currentSceneName   // Escena actual
int currentWaveNumber     // Oleada actual

// Configuración
float masterVolume        // Volumen
float mouseSensitivity    // Sensibilidad
```

## 🐛 Debug y Testing

### En Editor - SaveSystemDebugger

**Inspector:**
- Checkboxes para acciones rápidas
- Se auto-desactivan después de usar

**Context Menu:**
- Click derecho en componente
- "Quick Save", "Quick Load", etc.

**Teclas Rápidas:**
- `F5` - Quick Save
- `F9` - Quick Load

**Menú Tools:**
```
Tools/Save System/
├── Open Save Folder    → Abre carpeta de guardados
└── Show Save Path      → Muestra ruta en consola
```

### Archivos Guardados

**Ubicación:**
```
Windows: C:\Users\[User]\AppData\LocalLow\[Company]\[Game]\Saves\
Mac: ~/Library/Application Support/[Company]/[Game]/Saves/
```

**Formato:**
- Archivos `.json` (legibles)
- Nombre: `{slotName}.json`
- Ejemplo: `slot1.json`, `autosave.json`

## 📝 Flujo de Guardado

```
1. Jugador presiona "Guardar"
   ↓
2. SaveDataCollector.CollectAllData()
   - Llama a ISaveable.SaveData() en todos los componentes
   - Actualiza SaveSlotManager.CurrentGameData
   ↓
3. SaveSlotManager.SaveToSlot(slotName)
   - Actualiza saveDate en GameData
   - Llama a SaveSystem.SaveGame()
   ↓
4. SaveSystem.SaveGame()
   - Serializa GameData a JSON
   - Escribe archivo en disco
   ↓
5. OnGameSaved event se dispara
   - UI muestra "Guardado!"
```

## 📝 Flujo de Carga

```
1. Jugador selecciona slot
   ↓
2. SaveSlotManager.LoadFromSlot(slotName)
   - Llama a SaveSystem.LoadGame()
   ↓
3. SaveSystem.LoadGame()
   - Lee archivo JSON
   - Deserializa a GameData
   ↓
4. OnGameLoaded event se dispara
   ↓
5. SaveDataCollector.ApplyAllData()
   - Llama a ISaveable.LoadData() en todos los componentes
   - Restaura estado del juego
```

## ✅ Checklist de Implementación

### Setup Inicial
- [ ] Crear GameObject "SaveManager"
- [ ] Añadir SaveSlotManager
- [ ] Añadir SaveDataCollector
- [ ] Conectar referencias en Inspector
- [ ] Añadir SaveSystemDebugger (opcional)

### Hacer Jugador Guardable
- [ ] Añadir HealthSaveable al Player
- [ ] Añadir PlayerTransformSaveable al Player
- [ ] Testear con F5/F9 en Editor

### Integrar con UI
- [ ] Crear botones Guardar/Cargar
- [ ] Conectar a SaveSlotManager.SaveToSlot()
- [ ] Conectar a SaveSlotManager.LoadFromSlot()
- [ ] Conectar eventos OnGameSaved/OnGameLoaded

### Añadir Más Datos
- [ ] Extender GameData con nuevos campos
- [ ] Crear nuevos Saveable para otros componentes
- [ ] O modificar CurrentGameData directamente

## 🎯 Buenas Prácticas

### ✓ DO
- Usar ISaveable para componentes que cambien estado
- Guardar solo datos necesarios
- Validar datos al cargar (nulls, rangos)
- Testear guardado/carga frecuentemente
- Usar autosave para no perder progreso

### ✗ DON'T
- Guardar referencias a GameObjects (usa IDs o nombres)
- Guardar datos derivados (calcula en LoadData)
- Modificar GameData sin llamar a MarkAsModified()
- Ignorar los eventos OnSaveError
- Confiar en que los guardados siempre existen

## 🔄 Extender el Sistema

### Añadir Encriptación
```csharp
// En SaveSystem.cs, reemplazar:
File.WriteAllText(filePath, json);
// Por:
File.WriteAllText(filePath, Encrypt(json));
```

### Múltiples Perfiles de Usuario
```csharp
// Usar subfoldas por usuario:
private static string GetUserFolder(string userId)
{
    return Path.Combine(SaveFolderPath, userId);
}
```

### Guardado en la Nube
```csharp
// Implementar ICloudSaveProvider
// Subir/descargar JSON desde tu backend
```

## 📚 Referencia Rápida

| Acción | Código |
|--------|--------|
| Guardar | `saveSlotManager.SaveToSlot(1)` |
| Cargar | `saveSlotManager.LoadFromSlot(1)` |
| Nueva partida | `saveSlotManager.NewGame()` |
| Verificar slot | `saveSlotManager.SlotHasSave("slot1")` |
| Eliminar | `saveSlotManager.DeleteSlot("slot1")` |
| Datos actuales | `saveSlotManager.CurrentGameData` |
| Marcar modificado | `saveSlotManager.MarkAsModified()` |

## 🤝 Principios de Diseño

- **KISS**: Sistema simple, sin sobrecarga
- **YAGNI**: Solo lo necesario, extensible después
- **Modular**: Cada script una responsabilidad
- **Inspector-First**: Todo configurable visualmente
- **Event-Driven**: Reaccionar a guardado/carga vía UnityEvents

---

**¿Necesitas ayuda?**
- Revisa los metadatos al final de cada script
- Usa SaveSystemDebugger para testear
- Mira HealthSaveable como ejemplo de implementación
