using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Interfaz que deben implementar los MonoBehaviours que necesiten
    /// guardar y cargar sus datos. Facilita la recolección de datos distribuida.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Guarda el estado actual en el GameData proporcionado
        /// </summary>
        /// <param name="data">GameData donde guardar la información</param>
        void SaveData(GameData data);

        /// <summary>
        /// Carga el estado desde el GameData proporcionado
        /// </summary>
        /// <param name="data">GameData desde donde cargar la información</param>
        void LoadData(GameData data);
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Interfaz para implementar el patrón de persistencia distribuida.
 *   Los componentes que implementen esta interfaz pueden guardar/cargar
 *   automáticamente sus datos cuando se solicite.
 * 
 * RelatedScripts:
 *   - GameData.cs: Estructura donde se guardan los datos
 *   - SaveDataCollector.cs: Recolecta todos los ISaveable de la escena
 *   - Cualquier script que necesite persistencia (Health, Player, etc.)
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   None (es una interfaz)
 * 
 * SendsTo: 
 *   None (es una interfaz)
 * 
 * Setup:
 *   - No se adjunta directamente
 *   - Implementa en scripts que necesiten persistencia
 * 
 * Implementation Example:
 *   public class Health : MonoBehaviour, ISaveable
 *   {
 *       public float MaxHealth = 100f;
 *       public float CurrentHealth { get; set; }
 *       
 *       public void SaveData(GameData data)
 *       {
 *           data.playerHealth = CurrentHealth;
 *           data.playerMaxHealth = MaxHealth;
 *       }
 *       
 *       public void LoadData(GameData data)
 *       {
 *           MaxHealth = data.playerMaxHealth;
 *           CurrentHealth = data.playerHealth;
 *       }
 *   }
 * 
 * Notes:
 *   - Implementación opcional: Solo si necesitas lógica distribuida
 *   - Alternativa simple: Guardar/cargar todo desde SaveSlotManager directamente
 *   - Útil para proyectos grandes con muchos sistemas independientes
 * ============================================================================
 */
