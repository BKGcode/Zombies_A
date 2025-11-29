using System.Linq;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Recolecta datos de todos los componentes ISaveable en la escena
    /// y los guarda en GameData. Simplifica la persistencia distribuida.
    /// </summary>
    public class SaveDataCollector : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SaveSlotManager para acceder al GameData actual")]
        public SaveSlotManager saveSlotManager;

        [Header("Configuration")]
        [Tooltip("Recolectar datos automáticamente antes de guardar")]
        public bool autoCollectOnSave = true;

        [Tooltip("Aplicar datos automáticamente después de cargar")]
        public bool autoApplyOnLoad = true;

        private ISaveable[] saveableObjects;

        private void Awake()
        {
            if (saveSlotManager == null)
            {
                saveSlotManager = FindObjectOfType<SaveSlotManager>();
                
                if (saveSlotManager == null)
                {
                    Debug.LogError("[SaveDataCollector] No se encontró SaveSlotManager en la escena");
                    enabled = false;
                    return;
                }
            }

            CacheSaveableObjects();
        }

        private void OnEnable()
        {
            if (saveSlotManager == null)
                return;

            if (autoCollectOnSave)
            {
                saveSlotManager.OnGameSaved.AddListener(OnBeforeSave);
            }

            if (autoApplyOnLoad)
            {
                saveSlotManager.OnGameLoaded.AddListener(OnAfterLoad);
            }
        }

        private void OnDisable()
        {
            if (saveSlotManager == null)
                return;

            if (autoCollectOnSave)
            {
                saveSlotManager.OnGameSaved.RemoveListener(OnBeforeSave);
            }

            if (autoApplyOnLoad)
            {
                saveSlotManager.OnGameLoaded.RemoveListener(OnAfterLoad);
            }
        }

        /// <summary>
        /// Cachea todos los objetos ISaveable en la escena
        /// </summary>
        private void CacheSaveableObjects()
        {
            saveableObjects = FindObjectsOfType<MonoBehaviour>(true)
                .OfType<ISaveable>()
                .ToArray();

            Debug.Log($"[SaveDataCollector] Encontrados {saveableObjects.Length} objetos ISaveable");
        }

        /// <summary>
        /// Recolecta datos de todos los ISaveable antes de guardar
        /// </summary>
        private void OnBeforeSave(string slotName)
        {
            CollectAllData();
        }

        /// <summary>
        /// Aplica datos a todos los ISaveable después de cargar
        /// </summary>
        private void OnAfterLoad(GameData data)
        {
            ApplyAllData();
        }

        /// <summary>
        /// Recolecta datos de todos los objetos ISaveable y los guarda en CurrentGameData
        /// </summary>
        public void CollectAllData()
        {
            if (saveSlotManager == null || saveSlotManager.CurrentGameData == null)
            {
                Debug.LogWarning("[SaveDataCollector] No hay GameData disponible");
                return;
            }

            // Refrescar lista por si hay nuevos objetos
            CacheSaveableObjects();

            GameData currentData = saveSlotManager.CurrentGameData;

            foreach (ISaveable saveable in saveableObjects)
            {
                if (saveable != null)
                {
                    try
                    {
                        saveable.SaveData(currentData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SaveDataCollector] Error al guardar datos de {saveable}: {e.Message}");
                    }
                }
            }

            saveSlotManager.MarkAsModified();
            Debug.Log($"[SaveDataCollector] Datos recolectados de {saveableObjects.Length} objetos");
        }

        /// <summary>
        /// Aplica los datos del CurrentGameData a todos los objetos ISaveable
        /// </summary>
        public void ApplyAllData()
        {
            if (saveSlotManager == null || saveSlotManager.CurrentGameData == null)
            {
                Debug.LogWarning("[SaveDataCollector] No hay GameData disponible");
                return;
            }

            // Refrescar lista por si hay nuevos objetos
            CacheSaveableObjects();

            GameData currentData = saveSlotManager.CurrentGameData;

            foreach (ISaveable saveable in saveableObjects)
            {
                if (saveable != null)
                {
                    try
                    {
                        saveable.LoadData(currentData);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SaveDataCollector] Error al cargar datos en {saveable}: {e.Message}");
                    }
                }
            }

            Debug.Log($"[SaveDataCollector] Datos aplicados a {saveableObjects.Length} objetos");
        }

        /// <summary>
        /// Fuerza un refresh de la lista de ISaveable
        /// </summary>
        public void RefreshSaveableObjects()
        {
            CacheSaveableObjects();
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Orquestador que recolecta datos de todos los componentes ISaveable
 *   en la escena y los consolida en GameData antes de guardar.
 *   También distribuye datos cargados a todos los ISaveable.
 * 
 * RelatedScripts:
 *   - ISaveable.cs: Interfaz que implementan los objetos persistibles
 *   - SaveSlotManager.cs: Gestiona el guardado/carga
 *   - GameData.cs: Estructura de datos
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - SaveSlotManager.OnGameSaved
 *   - SaveSlotManager.OnGameLoaded
 * 
 * SendsTo: 
 *   None (modifica GameData directamente)
 * 
 * Setup:
 *   GameObject: "SaveDataCollector" (puede ser el mismo que SaveManager)
 *   
 *   Referencias en Inspector:
 *     - saveSlotManager: Arrastra el SaveSlotManager de la escena
 *   
 *   Configuración:
 *     - autoCollectOnSave: true (recolectar automáticamente antes de guardar)
 *     - autoApplyOnLoad: true (aplicar automáticamente después de cargar)
 * 
 * Usage:
 *   1. Asegúrate de que componentes como Health, Player, etc implementen ISaveable
 *   2. Este script los encontrará automáticamente (FindObjectsOfType)
 *   3. Antes de guardar: llama a todos los ISaveable.SaveData()
 *   4. Después de cargar: llama a todos los ISaveable.LoadData()
 * 
 * Notes:
 *   - Usa LINQ (System.Linq) para filtrar MonoBehaviours
 *   - Cachea la lista para performance
 *   - Incluye objetos inactivos (true en FindObjectsOfType)
 *   - Es opcional: si prefieres control manual, desactiva auto flags
 * ============================================================================
 */
