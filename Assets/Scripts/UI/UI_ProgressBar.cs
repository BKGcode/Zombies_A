using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GallinasFelices.UI
{
    public class UI_ProgressBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;

        public void Set(string label, float value, float maxValue)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }

            float fillAmount = maxValue > 0 ? value / maxValue : 0f;

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = fillAmount;
                slider.interactable = false;
            }

            if (fillImage != null && fillImage.type == Image.Type.Filled)
            {
                fillImage.fillAmount = fillAmount;
            }
        }
    }
}
