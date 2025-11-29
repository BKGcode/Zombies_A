using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Utilidades de debug para testear el sistema de guardado desde el Inspector o código.
    /// Solo para desarrollo, no usar en build final.
    /// </summary>
    public class SaveSystemDebugger : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SaveSlotManager a usar para testing")]
        public SaveSlotManager saveSlotManager;

        [Header("Test Configuration")]
        [Tooltip("Nombre del slot para tests rápidos")]
        public string testSlotName = "test_slot";

        [Header("Debug Actions")]
        [Tooltip("Guardar en el slot de prueba")]
        public bool saveTestSlot;

        [Tooltip("Cargar desde el slot de prueba")]
        public bool loadTestSlot;

        [Tooltip("Eliminar el slot de prueba")]
        public bool deleteTestSlot;

        [Tooltip("Listar todos los slots guardados")]
        public bool listAllSlots;

        [Tooltip("Imprimir datos actuales en consola")]
        public bool printCurrentData;

        [Tooltip("Eliminar TODOS los guardados")]
        public bool deleteAllSaves;

        private void OnValidate()
        {
            if (saveSlotManager == null)
            {
                saveSlotManager = FindObjectOfType<SaveSlotManager>();
            }
        }

        private void Update()
        {
            // Teclas rápidas para testear (solo en Editor)
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F5))
            {
                QuickSave();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                QuickLoad();
            }
            #endif

            ProcessDebugActions();
        }

        private void ProcessDebugActions()
        {
            if (saveSlotManager == null)
                return;

            if (saveTestSlot)
            {
                saveTestSlot = false;
                SaveToTestSlot();
            }

            if (loadTestSlot)
            {
                loadTestSlot = false;
                LoadFromTestSlot();
            }

            if (deleteTestSlot)
            {
                deleteTestSlot = false;
                DeleteTestSlot();
            }

            if (listAllSlots)
            {
                listAllSlots = false;
                ListAllSlots();
            }

            if (printCurrentData)
            {
                printCurrentData = false;
                PrintCurrentData();
            }

            if (deleteAllSaves)
            {
                deleteAllSaves = false;
                DeleteAll();
            }
        }

        [ContextMenu("Quick Save (F5)")]
        public void QuickSave()
        {
            if (saveSlotManager == null)
            {
                Debug.LogWarning("[SaveDebugger] No SaveSlotManager asignado");
                return;
            }

            bool success = saveSlotManager.SaveToSlot(testSlotName);
            if (success)
            {
                Debug.Log($"<color=green>[SaveDebugger] ✓ Guardado en '{testSlotName}'</color>");
            }
            else
            {
                Debug.LogError($"[SaveDebugger] ✗ Error al guardar en '{testSlotName}'");
            }
        }

        [ContextMenu("Quick Load (F9)")]
        public void QuickLoad()
        {
            if (saveSlotManager == null)
            {
                Debug.LogWarning("[SaveDebugger] No SaveSlotManager asignado");
                return;
            }

            bool success = saveSlotManager.LoadFromSlot(testSlotName);
            if (success)
            {
                Debug.Log($"<color=green>[SaveDebugger] ✓ Cargado desde '{testSlotName}'</color>");
            }
            else
            {
                Debug.LogError($"[SaveDebugger] ✗ Error al cargar desde '{testSlotName}'");
            }
        }

        [ContextMenu("Save to Test Slot")]
        public void SaveToTestSlot()
        {
            QuickSave();
        }

        [ContextMenu("Load from Test Slot")]
        public void LoadFromTestSlot()
        {
            QuickLoad();
        }

        [ContextMenu("Delete Test Slot")]
        public void DeleteTestSlot()
        {
            bool success = SaveSystem.DeleteSave(testSlotName);
            if (success)
            {
                Debug.Log($"<color=yellow>[SaveDebugger] Slot '{testSlotName}' eliminado</color>");
            }
            else
            {
                Debug.LogWarning($"[SaveDebugger] No se pudo eliminar '{testSlotName}'");
            }
        }

        [ContextMenu("List All Save Slots")]
        public void ListAllSlots()
        {
            string[] slots = SaveSystem.GetAllSaveSlots();

            if (slots.Length == 0)
            {
                Debug.Log("<color=yellow>[SaveDebugger] No hay guardados</color>");
                return;
            }

            Debug.Log($"<color=cyan>[SaveDebugger] Guardados encontrados ({slots.Length}):</color>");
            
            foreach (string slot in slots)
            {
                GameData data = SaveSystem.LoadGame(slot);
                if (data != null)
                {
                    Debug.Log($"  • {slot}: '{data.saveName}' - {data.saveDate} - {FormatPlayTime(data.totalPlayTime)}");
                }
                else
                {
                    Debug.Log($"  • {slot}: (error al leer)");
                }
            }
        }

        [ContextMenu("Print Current GameData")]
        public void PrintCurrentData()
        {
            if (saveSlotManager == null || saveSlotManager.CurrentGameData == null)
            {
                Debug.LogWarning("[SaveDebugger] No hay GameData disponible");
                return;
            }

            GameData data = saveSlotManager.CurrentGameData;
            string json = JsonUtility.ToJson(data, true);
            
            Debug.Log($"<color=cyan>[SaveDebugger] GameData actual:</color>\n{json}");
        }

        [ContextMenu("Delete ALL Saves (WARNING!)")]
        public void DeleteAll()
        {
            Debug.LogWarning("<color=red>[SaveDebugger] ⚠ ELIMINANDO TODOS LOS GUARDADOS ⚠</color>");
            SaveSystem.DeleteAllSaves();
        }

        [ContextMenu("New Game")]
        public void CreateNewGame()
        {
            if (saveSlotManager == null)
            {
                Debug.LogWarning("[SaveDebugger] No SaveSlotManager asignado");
                return;
            }

            saveSlotManager.NewGame("Debug Game");
            Debug.Log("<color=green>[SaveDebugger] Nueva partida creada</color>");
        }

        private string FormatPlayTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            int secs = (int)(seconds % 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else if (minutes > 0)
                return $"{minutes}m {secs}s";
            else
                return $"{secs}s";
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Herramienta de debug para testear el sistema de guardado en el Editor.
 *   Proporciona botones en Inspector y teclas rápidas para guardar/cargar.
 * 
 * RelatedScripts:
 *   - SaveSlotManager.cs: Manager principal
 *   - SaveSystem.cs: Sistema de archivos
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - Input (F5 para guardar, F9 para cargar)
 *   - Inspector toggles
 * 
 * SendsTo: 
 *   None
 * 
 * Setup:
 *   GameObject: "SaveDebugger" (mismo que SaveManager o separado)
 *   
 *   Referencias en Inspector:
 *     - saveSlotManager: Arrastra el SaveSlotManager
 *   
 *   Configuración:
 *     - testSlotName: Nombre del slot para tests (por defecto "test_slot")
 *   
 *   Uso en Editor:
 *     1. Click en checkboxes para ejecutar acciones
 *     2. Click derecho en componente → Context Menu
 *     3. F5 para guardar rápido, F9 para cargar rápido
 * 
 * Notes:
 *   - Solo para desarrollo/testing
 *   - Las teclas F5/F9 solo funcionan en Editor
 *   - Los toggles se auto-desactivan después de ejecutar
 *   - ⚠ Delete ALL Saves elimina TODO sin confirmación
 * ============================================================================
 */
