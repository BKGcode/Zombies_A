using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Events;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Gestor de pausa del juego que se integra con el sistema de tiempo.
    /// Maneja automáticamente la pausa cuando se abren menús o interfaces.
    /// </summary>
    public class GamePauseManager : MonoBehaviour
    {
        [Header("⚙️ Configuración")]
        [Tooltip("¿Pausar automáticamente el tiempo del juego?")]
        [SerializeField] private bool pauseGameTime = true;

        [Tooltip("Eventos que deben pausar el juego")]
        [SerializeField] private UnityEvent onPauseEvents;

        [Tooltip("Eventos que deben reanudar el juego")]
        [SerializeField] private UnityEvent onResumeEvents;

        [Header("🎮 Input")]
        [Tooltip("Tecla para pausar/reanudar manualmente")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        // Estado interno
        private TimeManager timeManager;
        private bool isGamePaused = false;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            InitializePauseSystem();
        }

        private void Update()
        {
            HandleManualPauseInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && pauseGameTime)
            {
                PauseGame();
            }
            else if (hasFocus && isGamePaused)
            {
                ResumeGame();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused && pauseGameTime)
            {
                PauseGame();
            }
        }

        #endregion

        #region Inicialización

        private void CacheComponents()
        {
            timeManager = TimeManager.Instance;
        }

        private void InitializePauseSystem()
        {
            // Conectar eventos de pausa/reanudar
            if (onPauseEvents != null)
            {
                onPauseEvents.AddListener(PauseGame);
            }

            if (onResumeEvents != null)
            {
                onResumeEvents.AddListener(ResumeGame);
            }
        }

        #endregion

        #region Control de Pausa

        /// <summary>
        /// Pausa el juego y el tiempo del sistema.
        /// </summary>
        public void PauseGame()
        {
            if (isGamePaused) return;

            isGamePaused = true;

            // Pausar tiempo del juego
            if (pauseGameTime)
            {
                Time.timeScale = 0f;

                // Pausar también el sistema de día/noche
                if (timeManager != null)
                {
                    timeManager.SetPaused(true);
                }
            }

            // Cursor visible y sin bloqueo
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("⏸️ Juego pausado");
        }

        /// <summary>
        /// Reanuda el juego y el tiempo del sistema.
        /// </summary>
        public void ResumeGame()
        {
            if (!isGamePaused) return;

            isGamePaused = false;

            // Reanudar tiempo del juego
            if (pauseGameTime)
            {
                Time.timeScale = 1f;

                // Reanudar también el sistema de día/noche
                if (timeManager != null)
                {
                    timeManager.SetPaused(false);
                }
            }

            // Cursor oculto y bloqueado (modo juego)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log("▶️ Juego reanudado");
        }

        /// <summary>
        /// Cambia el estado de pausa del juego.
        /// </summary>
        public void TogglePause()
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void HandleManualPauseInput()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }

        #endregion

        #region Consulta de Estado

        /// <summary>
        /// Verifica si el juego está pausado.
        /// </summary>
        public bool IsPaused()
        {
            return isGamePaused;
        }

        /// <summary>
        /// Obtiene el estado de la escala de tiempo.
        /// </summary>
        public float GetTimeScale()
        {
            return Time.timeScale;
        }

        #endregion

        #region Eventos Públicos

        /// <summary>
        /// Registra una acción que debe pausar el juego.
        /// </summary>
        public void RegisterPauseAction(UnityAction pauseAction)
        {
            if (pauseAction != null)
            {
                pauseAction += PauseGame;
            }
        }

        /// <summary>
        /// Registra una acción que debe reanudar el juego.
        /// </summary>
        public void RegisterResumeAction(UnityAction resumeAction)
        {
            if (resumeAction != null)
            {
                resumeAction += ResumeGame;
            }
        }

        #endregion
    }
}

