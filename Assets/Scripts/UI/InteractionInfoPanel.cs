using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace GallinasFelices.UI
{
    public class InteractionInfoPanel : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform barsContainer;
        [SerializeField] private GameObject barPrefab;

        [Header("Visual")]
        [SerializeField] private Image backgroundImage;

        public Core.IInteractable Target { get; private set; }
        public float TimeSinceLastTouch { get; private set; }

        private List<GameObject> activeButtons = new List<GameObject>();
        private List<GameObject> activeBars = new List<GameObject>();
        private Color originalColor;

        public void Initialize(Core.IInteractable target)
        {
            Target = target;
            
            if (backgroundImage != null)
            {
                originalColor = backgroundImage.color;
            }
            
            Refresh();
            
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }
            
            TimeSinceLastTouch = 0f;
        }

        public void Refresh()
        {
            if (Target == null) return;

            if (titleText != null)
            {
                titleText.text = Target.GetTitle();
            }

            if (infoText != null)
            {
                infoText.text = Target.GetMainInfo();
            }

            ClearButtons();
            CreateButtons(Target.GetActions());

            ClearBars();
            CreateBars(Target.GetBars());
        }

        public void OnTouched()
        {
            TimeSinceLastTouch = 0f;

            if (backgroundImage != null)
            {
                StartCoroutine(FlashBackground());
            }
        }

        private System.Collections.IEnumerator FlashBackground()
        {
            backgroundImage.color = Color.yellow;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                backgroundImage.color = Color.Lerp(Color.yellow, originalColor, elapsed / 0.2f);
                yield return null;
            }
            backgroundImage.color = originalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTouched();
        }

        private void Update()
        {
            TimeSinceLastTouch += Time.deltaTime;
        }

        private void ClearButtons()
        {
            foreach (GameObject button in activeButtons)
            {
                Destroy(button);
            }
            activeButtons.Clear();
        }

        private void ClearBars()
        {
            foreach (GameObject bar in activeBars)
            {
                Destroy(bar);
            }
            activeBars.Clear();
        }

        private void CreateButtons(Core.InteractionButton[] buttons)
        {
            if (buttons == null || buttonPrefab == null || buttonContainer == null) return;

            foreach (Core.InteractionButton buttonData in buttons)
            {
                GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = buttonData.label;
                }

                if (button != null)
                {
                    button.interactable = buttonData.isEnabled;
                    
                    if (buttonData.onClick != null)
                    {
                        button.onClick.AddListener(() =>
                        {
                            buttonData.onClick.Invoke();
                            Refresh();
                            OnTouched();
                        });
                    }
                }

                activeButtons.Add(buttonObj);
            }
        }

        private void CreateBars(Core.InteractionBar[] bars)
        {
            if (bars == null || barPrefab == null || barsContainer == null) return;

            foreach (Core.InteractionBar barData in bars)
            {
                GameObject barObj = Instantiate(barPrefab, barsContainer);
                
                // Try to get the dedicated controller first
                UI_ProgressBar progressBar = barObj.GetComponent<UI_ProgressBar>();
                if (progressBar != null)
                {
                    progressBar.Set(barData.label, barData.value, barData.maxValue);
                }
                else
                {
                    // Fallback to legacy logic (Scrollbar OR Slider)
                    TextMeshProUGUI labelText = barObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (labelText != null) labelText.text = barData.label;

                    Scrollbar scrollbar = barObj.GetComponent<Scrollbar>();
                    if (scrollbar != null)
                    {
                        scrollbar.size = barData.maxValue > 0 ? barData.value / barData.maxValue : 0f;
                        scrollbar.interactable = false;
                    }
                    else
                    {
                        Slider slider = barObj.GetComponent<Slider>();
                        if (slider != null)
                        {
                            slider.value = barData.maxValue > 0 ? barData.value / barData.maxValue : 0f;
                            slider.interactable = false;
                        }
                    }
                }

                activeBars.Add(barObj);
            }
        }

        private void OnCloseClicked()
        {
            if (Core.PanelManager.Instance != null)
            {
                Core.PanelManager.Instance.RemovePanel(this);
            }
        }
    }
}
