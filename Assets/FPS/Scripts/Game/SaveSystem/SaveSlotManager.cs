using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Gestiona los slots de guardado y proporciona una interfaz simple para guardar/cargar.
    /// Debe existir uno por escena o ser persistente entre escenas.
    /// </summary>
    public class SaveSlotManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Número máximo de slots de guardado disponibles")]
        [Range(1, 10)]
        public int maxSlots = 3;

        [Tooltip("Nombre del slot para autosave")]
        public string autoSaveSlotName = "autosave";

        [Tooltip("Intervalo en segundos para autosave (0 = desactivado)")]
        public float autoSaveInterval = 300f; // 5 minutos

        [Header("Events")]
        [Tooltip("Se invoca cuando se guarda exitosamente")]
        public UnityEvent<string> OnGameSaved;

        [Tooltip("Se invoca cuando se carga exitosamente")]
        public UnityEvent<GameData> OnGameLoaded;

        [Tooltip("Se invoca cuando falla una operación")]
        public UnityEvent<string> OnSaveError;

        // Estado actual
        public GameData CurrentGameData { get; private set; }
        public string CurrentSlotName { get; private set; }
        public bool HasUnsavedChanges { get; private set; }

        private float autoSaveTimer;

        private void Awake()
        {
            SaveSystem.Initialize();
            CurrentGameData = GameData.CreateDefault();
        }

        private void Start()
        {
            autoSaveTimer = autoSaveInterval;
        }

        private void Update()
        {
            UpdateAutoSave();
            UpdatePlayTime();
        }

        /// <summary>
        /// Actualiza el tiempo de juego
        /// </summary>
        private void UpdatePlayTime()
        {
            if (CurrentGameData != null)
            {
                CurrentGameData.totalPlayTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// Maneja el autosave periódico
        /// </summary>
        private void UpdateAutoSave()
        {
            if (autoSaveInterval <= 0)
                return;

            autoSaveTimer -= Time.deltaTime;

            if (autoSaveTimer <= 0)
            {
                AutoSave();
                autoSaveTimer = autoSaveInterval;
            }
        }

        /// <summary>
        /// Guarda la partida en el slot especificado
        /// </summary>
        public bool SaveToSlot(string slotName)
        {
            if (CurrentGameData == null)
            {
                Debug.LogWarning("[SaveSlotManager] No hay datos para guardar");
                OnSaveError?.Invoke("No hay datos para guardar");
                return false;
            }

            bool success = SaveSystem.SaveGame(slotName, CurrentGameData);

            if (success)
            {
                CurrentSlotName = slotName;
                HasUnsavedChanges = false;
                OnGameSaved?.Invoke(slotName);
                Debug.Log($"[SaveSlotManager] Partida guardada en slot: {slotName}");
            }
            else
            {
                OnSaveError?.Invoke($"Error al guardar en {slotName}");
            }

            return success;
        }

        /// <summary>
        /// Guarda en el slot numerado (slot1, slot2, etc)
        /// </summary>
        public bool SaveToSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > maxSlots)
            {
                Debug.LogWarning($"[SaveSlotManager] Slot index fuera de rango: {slotIndex}");
                return false;
            }

            return SaveToSlot($"slot{slotIndex}");
        }

        /// <summary>
        /// Carga la partida desde el slot especificado
        /// </summary>
        public bool LoadFromSlot(string slotName)
        {
            GameData loadedData = SaveSystem.LoadGame(slotName);

            if (loadedData == null)
            {
                OnSaveError?.Invoke($"No se pudo cargar {slotName}");
                return false;
            }

            CurrentGameData = loadedData;
            CurrentSlotName = slotName;
            HasUnsavedChanges = false;
            OnGameLoaded?.Invoke(loadedData);
            Debug.Log($"[SaveSlotManager] Partida cargada desde slot: {slotName}");
            return true;
        }

        /// <summary>
        /// Carga desde el slot numerado (slot1, slot2, etc)
        /// </summary>
        public bool LoadFromSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > maxSlots)
            {
                Debug.LogWarning($"[SaveSlotManager] Slot index fuera de rango: {slotIndex}");
                return false;
            }

            return LoadFromSlot($"slot{slotIndex}");
        }

        /// <summary>
        /// Elimina el guardado de un slot
        /// </summary>
        public bool DeleteSlot(string slotName)
        {
            bool success = SaveSystem.DeleteSave(slotName);

            if (success && CurrentSlotName == slotName)
            {
                CurrentSlotName = null;
            }

            return success;
        }

        /// <summary>
        /// Verifica si existe guardado en un slot
        /// </summary>
        public bool SlotHasSave(string slotName)
        {
            return SaveSystem.SaveExists(slotName);
        }

        /// <summary>
        /// Obtiene los datos de un slot sin cargarlo
        /// </summary>
        public GameData PreviewSlot(string slotName)
        {
            return SaveSystem.LoadGame(slotName);
        }

        /// <summary>
        /// Guarda automáticamente en el slot de autosave
        /// </summary>
        public void AutoSave()
        {
            if (CurrentGameData == null)
                return;

            SaveToSlot(autoSaveSlotName);
            Debug.Log("[SaveSlotManager] Autosave completado");
        }

        /// <summary>
        /// Crea una nueva partida con datos por defecto
        /// </summary>
        public void NewGame(string saveName = "New Game")
        {
            CurrentGameData = GameData.CreateDefault();
            CurrentGameData.saveName = saveName;
            CurrentSlotName = null;
            HasUnsavedChanges = true;
            Debug.Log("[SaveSlotManager] Nueva partida creada");
        }

        /// <summary>
        /// Marca que hay cambios sin guardar
        /// </summary>
        public void MarkAsModified()
        {
            HasUnsavedChanges = true;
        }

        /// <summary>
        /// Obtiene todos los slots con guardados
        /// </summary>
        public string[] GetAllSavedSlots()
        {
            return SaveSystem.GetAllSaveSlots();
        }

        private void OnApplicationQuit()
        {
            // Autosave al cerrar el juego si está habilitado
            if (autoSaveInterval > 0 && CurrentGameData != null)
            {
                AutoSave();
            }
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Manager que gestiona múltiples slots de guardado y proporciona funciones
 *   de alto nivel para guardar/cargar partidas. Maneja autosave y eventos.
 * 
 * RelatedScripts:
 *   - SaveSystem.cs: Sistema de bajo nivel para I/O de archivos
 *   - GameData.cs: Estructura de datos que se guarda
 *   - SaveLoadUI.cs: UI para mostrar y seleccionar slots
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - UI buttons (SaveToSlot, LoadFromSlot, DeleteSlot)
 *   - Game events (cuando cambian datos a guardar)
 * 
 * SendsTo: 
 *   - OnGameSaved: string slotName
 *   - OnGameLoaded: GameData
 *   - OnSaveError: string errorMessage
 * 
 * Setup:
 *   GameObject: "SaveManager" (vacío, persistente con DontDestroyOnLoad si es necesario)
 *   Referencias en Inspector:
 *     - maxSlots: Configura cuántos slots quieres (por defecto 3)
 *     - autoSaveSlotName: Nombre del slot de autosave
 *     - autoSaveInterval: Segundos entre autosaves (0 = desactivado)
 *   
 *   Conecta UnityEvents a UI o sistemas que necesiten reaccionar:
 *     - OnGameSaved: Para mostrar mensaje "Guardado"
 *     - OnGameLoaded: Para aplicar los datos cargados al juego
 *     - OnSaveError: Para mostrar errores al jugador
 * 
 * Usage Example:
 *   // Guardar en slot 1
 *   saveSlotManager.SaveToSlot(1);
 *   
 *   // Cargar desde slot 2
 *   saveSlotManager.LoadFromSlot(2);
 *   
 *   // Verificar si hay guardado
 *   if (saveSlotManager.SlotHasSave("slot1")) { ... }
 * 
 * Notes:
 *   - CurrentGameData contiene los datos actuales en memoria
 *   - Llama a MarkAsModified() cuando cambien datos importantes
 *   - Autosave se ejecuta periódicamente y al cerrar el juego
 * ============================================================================
 */
