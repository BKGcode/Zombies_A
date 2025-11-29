using System;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Contiene toda la información de una partida guardada.
    /// Esta clase es serializable a JSON.
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        [Header("Save Info")]
        public string saveName = "New Save";
        public string saveDate;
        public float totalPlayTime;

        [Header("Player Stats")]
        public float playerHealth;
        public float playerMaxHealth;
        public Vector3 playerPosition;
        public Vector3 playerRotation;

        [Header("Weapons")]
        public string[] unlockedWeapons = new string[0];
        public int activeWeaponIndex;

        [Header("Game Progress")]
        public int enemiesKilled;
        public int objectivesCompleted;
        public string currentSceneName;
        public int currentWaveNumber;

        [Header("Settings")]
        public float masterVolume = 1f;
        public float mouseSensitivity = 1f;

        /// <summary>
        /// Crea un nuevo GameData con valores por defecto
        /// </summary>
        public static GameData CreateDefault()
        {
            return new GameData
            {
                saveName = "New Game",
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalPlayTime = 0f,
                playerHealth = 100f,
                playerMaxHealth = 100f,
                playerPosition = Vector3.zero,
                playerRotation = Vector3.zero,
                unlockedWeapons = new string[0],
                activeWeaponIndex = 0,
                enemiesKilled = 0,
                objectivesCompleted = 0,
                currentSceneName = "",
                currentWaveNumber = 0,
                masterVolume = 1f,
                mouseSensitivity = 1f
            };
        }

        /// <summary>
        /// Actualiza la fecha y hora de guardado
        /// </summary>
        public void UpdateSaveDate()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Clase de datos serializable que contiene toda la información de una partida.
 *   No hereda de MonoBehaviour, es un simple contenedor de datos.
 * 
 * RelatedScripts:
 *   - SaveSystem.cs: Sistema que guarda/carga instancias de GameData
 *   - SaveSlotManager.cs: Gestiona múltiples slots de guardado
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   None (es solo datos)
 * 
 * SendsTo: 
 *   None (es solo datos)
 * 
 * Setup:
 *   - No se adjunta a ningún GameObject
 *   - Se serializa a JSON para guardar en disco
 *   - Extensible: Añade más campos según necesites
 * 
 * Notes:
 *   - Usa tipos simples serializables (int, float, string, Vector3)
 *   - Para arrays/listas complejas, considera crear clases serializables anidadas
 *   - Todos los campos son públicos para serialización JSON
 * ============================================================================
 */
