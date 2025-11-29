using System;
using System.IO;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Sistema central de guardado y carga de partidas.
    /// Maneja la serialización/deserialización de GameData a JSON.
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_FOLDER = "Saves";
        private const string FILE_EXTENSION = ".json";

        private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        /// <summary>
        /// Inicializa el sistema de guardado (crea la carpeta si no existe)
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
                Debug.Log($"[SaveSystem] Carpeta de guardado creada en: {SaveFolderPath}");
            }
        }

        /// <summary>
        /// Guarda los datos en un slot específico
        /// </summary>
        /// <param name="slotName">Nombre del slot (ej: "slot1", "autosave")</param>
        /// <param name="data">Datos a guardar</param>
        /// <returns>True si se guardó correctamente</returns>
        public static bool SaveGame(string slotName, GameData data)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogWarning("[SaveSystem] Nombre de slot vacío");
                return false;
            }

            if (data == null)
            {
                Debug.LogWarning("[SaveSystem] GameData es null");
                return false;
            }

            try
            {
                Initialize();

                data.UpdateSaveDate();
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetSaveFilePath(slotName);

                File.WriteAllText(filePath, json);
                Debug.Log($"[SaveSystem] Partida guardada en: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error al guardar: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga los datos de un slot específico
        /// </summary>
        /// <param name="slotName">Nombre del slot a cargar</param>
        /// <returns>GameData cargado, o null si no existe o hay error</returns>
        public static GameData LoadGame(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogWarning("[SaveSystem] Nombre de slot vacío");
                return null;
            }

            try
            {
                string filePath = GetSaveFilePath(slotName);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] No existe guardado en slot: {slotName}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"[SaveSystem] Partida cargada desde: {filePath}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error al cargar: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Elimina un guardado específico
        /// </summary>
        /// <param name="slotName">Nombre del slot a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        public static bool DeleteSave(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogWarning("[SaveSystem] Nombre de slot vacío");
                return false;
            }

            try
            {
                string filePath = GetSaveFilePath(slotName);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] No existe guardado para eliminar: {slotName}");
                    return false;
                }

                File.Delete(filePath);
                Debug.Log($"[SaveSystem] Guardado eliminado: {slotName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error al eliminar: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un guardado en el slot especificado
        /// </summary>
        /// <param name="slotName">Nombre del slot a verificar</param>
        /// <returns>True si existe el guardado</returns>
        public static bool SaveExists(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
                return false;

            string filePath = GetSaveFilePath(slotName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Obtiene todos los nombres de slots guardados
        /// </summary>
        /// <returns>Array con nombres de slots (sin extensión)</returns>
        public static string[] GetAllSaveSlots()
        {
            Initialize();

            if (!Directory.Exists(SaveFolderPath))
                return new string[0];

            string[] files = Directory.GetFiles(SaveFolderPath, $"*{FILE_EXTENSION}");
            string[] slotNames = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                slotNames[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return slotNames;
        }

        /// <summary>
        /// Obtiene la ruta completa del archivo de guardado
        /// </summary>
        private static string GetSaveFilePath(string slotName)
        {
            return Path.Combine(SaveFolderPath, slotName + FILE_EXTENSION);
        }

        /// <summary>
        /// Elimina todos los guardados (usar con precaución)
        /// </summary>
        public static void DeleteAllSaves()
        {
            if (Directory.Exists(SaveFolderPath))
            {
                Directory.Delete(SaveFolderPath, true);
                Debug.Log("[SaveSystem] Todos los guardados eliminados");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Abre la carpeta de guardados en el explorador (solo en Editor)
        /// </summary>
        [UnityEditor.MenuItem("Tools/Save System/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            Initialize();
            System.Diagnostics.Process.Start(SaveFolderPath);
        }

        /// <summary>
        /// Muestra la ruta de guardado en consola (solo en Editor)
        /// </summary>
        [UnityEditor.MenuItem("Tools/Save System/Show Save Path")]
        public static void ShowSavePath()
        {
            Initialize();
            Debug.Log($"[SaveSystem] Ruta de guardado: {SaveFolderPath}");
        }
#endif
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Sistema estático que maneja la serialización y persistencia de GameData.
 *   Proporciona métodos simples para guardar/cargar/eliminar partidas.
 * 
 * RelatedScripts:
 *   - GameData.cs: Estructura de datos que se serializa
 *   - SaveSlotManager.cs: UI y gestión de múltiples slots
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   None
 * 
 * SendsTo: 
 *   None
 * 
 * Setup:
 *   - No se adjunta a ningún GameObject (es static)
 *   - Guarda archivos en Application.persistentDataPath/Saves/
 *   - Formato: JSON legible con JsonUtility
 *   - En Editor: Menú Tools/Save System/ para gestionar guardados
 * 
 * Notes:
 *   - Thread-safe: NO. Llámalo solo desde el main thread
 *   - Los guardados se nombran como: "slotName.json"
 *   - Usa JsonUtility (simple, pero limitado a tipos serializables)
 *   - Para encriptación, modifica WriteAllText/ReadAllText
 *   - Persistente entre sesiones del juego
 * ============================================================================
 */
