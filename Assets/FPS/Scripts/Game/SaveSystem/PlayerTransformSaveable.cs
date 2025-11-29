using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Guarda y carga la posición y rotación del Transform del jugador.
    /// </summary>
    public class PlayerTransformSaveable : MonoBehaviour, ISaveable
    {
        [Header("Configuration")]
        [Tooltip("Solo aplicar a objetos con tag 'Player'")]
        public bool onlyForPlayer = true;

        [Tooltip("Guardar posición")]
        public bool savePosition = true;

        [Tooltip("Guardar rotación")]
        public bool saveRotation = true;

        private Transform cachedTransform;
        private bool shouldSave;

        private void Awake()
        {
            cachedTransform = transform;
            shouldSave = !onlyForPlayer || CompareTag("Player");
        }

        public void SaveData(GameData data)
        {
            if (!shouldSave)
                return;

            if (savePosition)
            {
                data.playerPosition = cachedTransform.position;
            }

            if (saveRotation)
            {
                data.playerRotation = cachedTransform.eulerAngles;
            }
        }

        public void LoadData(GameData data)
        {
            if (!shouldSave)
                return;

            if (savePosition)
            {
                cachedTransform.position = data.playerPosition;
            }

            if (saveRotation)
            {
                cachedTransform.eulerAngles = data.playerRotation;
            }
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Guarda y restaura la posición/rotación del jugador.
 *   Útil para continuar partidas desde donde se guardó.
 * 
 * RelatedScripts:
 *   - ISaveable.cs: Interfaz implementada
 *   - SaveDataCollector.cs: Recolecta datos
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - SaveDataCollector
 * 
 * SendsTo: 
 *   None
 * 
 * Setup:
 *   GameObject: Player (el transform principal del jugador)
 *   
 *   Configuración:
 *     - onlyForPlayer: true (verificar tag "Player")
 *     - savePosition: true
 *     - saveRotation: true
 * 
 * Notes:
 *   - Cachea el transform en Awake para performance
 *   - Guarda rotación como eulerAngles (más legible en JSON)
 *   - Si usas CharacterController, considera deshabilitarlo antes de SetPosition
 * ============================================================================
 */
