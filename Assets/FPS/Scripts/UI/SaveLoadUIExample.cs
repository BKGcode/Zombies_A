using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Ejemplo de UI simple para guardar/cargar partidas.
    /// Muestra cómo integrar el sistema de guardado con la interfaz.
    /// </summary>
    public class SaveLoadUIExample : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SaveSlotManager del juego")]
        public SaveSlotManager saveSlotManager;

        [Header("UI Elements - Save")]
        [Tooltip("Botones para guardar en cada slot")]
        public Button[] saveButtons = new Button[3];

        [Header("UI Elements - Load")]
        [Tooltip("Botones para cargar desde cada slot")]
        public Button[] loadButtons = new Button[3];

        [Header("UI Elements - Info")]
        [Tooltip("Textos para mostrar info de cada slot")]
        public TextMeshProUGUI[] slotInfoTexts = new TextMeshProUGUI[3];

        [Header("UI Elements - Feedback")]
        [Tooltip("Texto para mostrar mensajes al jugador")]
        public TextMeshProUGUI feedbackText;

        [Tooltip("Duración del mensaje de feedback")]
        public float feedbackDuration = 2f;

        private float feedbackTimer;

        private void Start()
        {
            if (saveSlotManager == null)
            {
                saveSlotManager = FindObjectOfType<SaveSlotManager>();
            }

            SetupButtons();
            RefreshSlotInfo();

            // Suscribirse a eventos
            if (saveSlotManager != null)
            {
                saveSlotManager.OnGameSaved.AddListener(OnGameSaved);
                saveSlotManager.OnGameLoaded.AddListener(OnGameLoaded);
                saveSlotManager.OnSaveError.AddListener(OnSaveError);
            }
        }

        private void OnDestroy()
        {
            if (saveSlotManager != null)
            {
                saveSlotManager.OnGameSaved.RemoveListener(OnGameSaved);
                saveSlotManager.OnGameLoaded.RemoveListener(OnGameLoaded);
                saveSlotManager.OnSaveError.RemoveListener(OnSaveError);
            }
        }

        private void Update()
        {
            UpdateFeedbackText();
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupButtons()
        {
            // Botones de guardar
            for (int i = 0; i < saveButtons.Length; i++)
            {
                if (saveButtons[i] != null)
                {
                    int slotIndex = i + 1; // slots 1-3
                    saveButtons[i].onClick.AddListener(() => SaveToSlot(slotIndex));
                }
            }

            // Botones de cargar
            for (int i = 0; i < loadButtons.Length; i++)
            {
                if (loadButtons[i] != null)
                {
                    int slotIndex = i + 1;
                    loadButtons[i].onClick.AddListener(() => LoadFromSlot(slotIndex));
                }
            }
        }

        /// <summary>
        /// Actualiza la información mostrada de cada slot
        /// </summary>
        public void RefreshSlotInfo()
        {
            for (int i = 0; i < slotInfoTexts.Length; i++)
            {
                if (slotInfoTexts[i] == null)
                    continue;

                string slotName = $"slot{i + 1}";
                GameData slotData = saveSlotManager?.PreviewSlot(slotName);

                if (slotData != null)
                {
                    // Hay datos guardados
                    string info = $"<b>{slotData.saveName}</b>\n";
                    info += $"{slotData.saveDate}\n";
                    info += $"Tiempo: {FormatPlayTime(slotData.totalPlayTime)}";
                    slotInfoTexts[i].text = info;

                    // Habilitar botón de cargar
                    if (i < loadButtons.Length && loadButtons[i] != null)
                    {
                        loadButtons[i].interactable = true;
                    }
                }
                else
                {
                    // Slot vacío
                    slotInfoTexts[i].text = "<color=#888888>Slot vacío</color>";

                    // Deshabilitar botón de cargar
                    if (i < loadButtons.Length && loadButtons[i] != null)
                    {
                        loadButtons[i].interactable = false;
                    }
                }
            }
        }

        /// <summary>
        /// Guarda en un slot específico
        /// </summary>
        private void SaveToSlot(int slotIndex)
        {
            if (saveSlotManager == null)
                return;

            bool success = saveSlotManager.SaveToSlot(slotIndex);
            
            if (success)
            {
                RefreshSlotInfo();
            }
        }

        /// <summary>
        /// Carga desde un slot específico
        /// </summary>
        private void LoadFromSlot(int slotIndex)
        {
            if (saveSlotManager == null)
                return;

            bool success = saveSlotManager.LoadFromSlot(slotIndex);
            
            if (success)
            {
                // Aquí puedes cambiar de escena, cerrar menú, etc.
                // SceneManager.LoadScene(saveSlotManager.CurrentGameData.currentSceneName);
            }
        }

        /// <summary>
        /// Callback cuando se guarda exitosamente
        /// </summary>
        private void OnGameSaved(string slotName)
        {
            ShowFeedback($"✓ Partida guardada en {slotName}", Color.green);
        }

        /// <summary>
        /// Callback cuando se carga exitosamente
        /// </summary>
        private void OnGameLoaded(GameData data)
        {
            ShowFeedback($"✓ Partida cargada: {data.saveName}", Color.cyan);
        }

        /// <summary>
        /// Callback cuando hay un error
        /// </summary>
        private void OnSaveError(string error)
        {
            ShowFeedback($"✗ Error: {error}", Color.red);
        }

        /// <summary>
        /// Muestra un mensaje temporal al jugador
        /// </summary>
        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText == null)
                return;

            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
            feedbackTimer = feedbackDuration;
        }

        /// <summary>
        /// Actualiza el temporizador del mensaje de feedback
        /// </summary>
        private void UpdateFeedbackText()
        {
            if (feedbackText == null || !feedbackText.gameObject.activeSelf)
                return;

            feedbackTimer -= Time.deltaTime;

            if (feedbackTimer <= 0)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Formatea el tiempo de juego en formato legible
        /// </summary>
        private string FormatPlayTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        /// <summary>
        /// Botón público para nueva partida (asignar desde Inspector)
        /// </summary>
        public void OnNewGameButton()
        {
            if (saveSlotManager != null)
            {
                saveSlotManager.NewGame("Nueva Partida");
                ShowFeedback("Nueva partida iniciada", Color.white);
            }
        }
    }
}

/*
 * ============================================================================
 * METADATA
 * ============================================================================
 * ScriptRole: 
 *   Ejemplo de interfaz de usuario para el sistema de guardado.
 *   Muestra cómo integrar SaveSlotManager con botones y textos UI.
 * 
 * RelatedScripts:
 *   - SaveSlotManager.cs: Gestiona las operaciones de guardado
 *   - GameData.cs: Datos mostrados en la UI
 * 
 * UsesSO: 
 *   None
 * 
 * ReceivesFrom: 
 *   - UI Buttons (onClick events)
 *   - SaveSlotManager.OnGameSaved
 *   - SaveSlotManager.OnGameLoaded
 *   - SaveSlotManager.OnSaveError
 * 
 * SendsTo: 
 *   None
 * 
 * Setup:
 *   GameObject: Canvas con este script (ej: "SaveLoadMenu")
 *   
 *   UI Structure:
 *   Canvas
 *   ├── SaveLoadMenu (este script)
 *   ├── Slot1Panel
 *   │   ├── SaveButton1
 *   │   ├── LoadButton1
 *   │   └── InfoText1
 *   ├── Slot2Panel
 *   │   ├── SaveButton2
 *   │   ├── LoadButton2
 *   │   └── InfoText2
 *   ├── Slot3Panel
 *   │   ├── SaveButton3
 *   │   ├── LoadButton3
 *   │   └── InfoText3
 *   └── FeedbackText (mensajes temporales)
 *   
 *   Referencias en Inspector:
 *     - saveSlotManager: Arrastra el SaveSlotManager
 *     - saveButtons[]: Arrastra los 3 botones de guardar
 *     - loadButtons[]: Arrastra los 3 botones de cargar
 *     - slotInfoTexts[]: Arrastra los 3 textos de info
 *     - feedbackText: Texto para mensajes temporales
 * 
 * Notes:
 *   - Los botones de cargar se deshabilitan si el slot está vacío
 *   - Los mensajes de feedback desaparecen después de feedbackDuration
 *   - Llama a RefreshSlotInfo() después de guardar/eliminar
 *   - Usa TextMeshPro (TMP) para mejor calidad de texto
 *   - Es un EJEMPLO: personalízalo según tu diseño de UI
 * ============================================================================
 */
