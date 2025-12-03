using UnityEngine;
using GallinasFelices.Structures;

namespace GallinasFelices.VFX
{
    public class CoopFullIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Coop coop;
        [SerializeField] private GameObject fullIconPrefab;

        [Header("Settings")]
        [SerializeField] private Vector3 iconOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float checkInterval = 1f;

        private GameObject currentIcon;
        private float checkTimer;

        private void Awake()
        {
            if (coop == null)
            {
                coop = GetComponent<Coop>();
            }
            
            checkTimer = checkInterval;
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
            bool shouldShowIcon = coop != null && !coop.HasAvailableSpot();

            if (shouldShowIcon && currentIcon == null)
            {
                ShowFullIcon();
            }
            else if (!shouldShowIcon && currentIcon != null)
            {
                HideIcon();
            }

            if (currentIcon != null)
            {
                currentIcon.transform.position = transform.position + iconOffset;
            }
        }

        private void ShowFullIcon()
        {
            if (fullIconPrefab == null) return;

            currentIcon = Instantiate(fullIconPrefab, transform.position + iconOffset, Quaternion.identity);
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
