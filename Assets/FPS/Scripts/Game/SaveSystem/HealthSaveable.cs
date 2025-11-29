using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Ejemplo de implementación de ISaveable para el componente Health del jugador.
    /// Guarda y carga la salud actual y máxima.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class HealthSaveable : MonoBehaviour, ISaveable
    {
        [Header("References")]
        [Tooltip("Componente Health a guardar/cargar")]
        public Health health;

        [Header("Configuration")]
        [Tooltip("Solo guardar si es del jugador (tag 'Player')")]
        public bool onlyForPlayer = true;

        private bool shouldSave;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            // Determinar si este objeto debe guardarse
            shouldSave = !onlyForPlayer || CompareTag("Player");
        }

        public void SaveData(GameData data)
        {
            if (!shouldSave || health == null)
                return;

            data.playerHealth = health.CurrentHealth;
            data.playerMaxHealth = health.MaxHealth;
        }

        public void LoadData(GameData data)
        {
            if (!shouldSave || health == null)
                return;

            health.MaxHealth = data.playerMaxHealth;
            health.CurrentHealth = data.playerHealth;
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Adaptador que hace que el componente Health sea guardable.
 *   Implementa ISaveable para integrarse con el sistema de guardado.
 * 
 * RelatedScripts:
 *   - Health.cs: Componente que se guarda
 *   - ISaveable.cs: Interfaz implementada
 *   - SaveDataCollector.cs: Recolecta datos de este componente
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - SaveDataCollector (llamadas a SaveData/LoadData)
 * 
 * SendsTo: 
 *   None
 * 
 * Setup:
 *   GameObject: El mismo GameObject que tiene Health (típicamente el Player)
 *   
 *   Referencias en Inspector:
 *     - health: Auto-asignado en Awake si está vacío
 *   
 *   Configuración:
 *     - onlyForPlayer: true si solo quieres guardar la salud del jugador
 * 
 * Notes:
 *   - Patrón Adapter: Separa la lógica de guardado de la lógica de juego
 *   - RequireComponent asegura que Health existe
 *   - Este es un EJEMPLO, puedes crear más Saveable para otros componentes
 * ============================================================================
 */
