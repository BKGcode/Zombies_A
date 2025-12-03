using UnityEngine;
using GallinasFelices.Chicken;

namespace GallinasFelices.VFX
{
    public class ChickenWarningIcon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Chicken.Chicken chicken;
        [SerializeField] private GameObject coldIconPrefab;

        [Header("Settings")]
        [SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private float checkInterval = 0.5f;

        private GameObject currentIcon;
        private float checkTimer;

        private void Awake()
        {
            if (chicken == null)
            {
                chicken = GetComponent<Chicken.Chicken>();
            }
        }

        private void Update()
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                UpdateWarningIcon();
                checkTimer = checkInterval;
            }
        }

        private void UpdateWarningIcon()
        {
            bool shouldShowColdIcon = chicken.CurrentState == ChickenState.Sleeping && chicken.IsSleepingOutside;

            if (shouldShowColdIcon && currentIcon == null)
            {
                ShowColdIcon();
            }
            else if (!shouldShowColdIcon && currentIcon != null)
            {
                HideIcon();
            }

            if (currentIcon != null)
            {
                currentIcon.transform.position = transform.position + iconOffset;
            }
        }

        private void ShowColdIcon()
        {
            if (coldIconPrefab == null) return;

            currentIcon = Instantiate(coldIconPrefab, transform.position + iconOffset, Quaternion.identity);
            currentIcon.transform.SetParent(transform);
            currentIcon.SetActive(true);
        }

        private void HideIcon()
        {
            if (currentIcon != null)
            {
                Destroy(currentIcon);
                currentIcon = null;
            }
        }

        private void OnDestroy()
        {
            HideIcon();
        }

        private void OnDisable()
        {
            HideIcon();
        }
    }
}
