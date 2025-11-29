using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace GallinasFelices.Core
{
    public enum PanelLane
    {
        Left,
        Right
    }

    public class PanelManager : MonoBehaviour
    {
        public static PanelManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UI.InteractionInfoPanel panelPrefab;
        [SerializeField] private Transform leftLaneContainer;
        [SerializeField] private Transform rightLaneContainer;

        [Header("Settings")]
        [SerializeField] private float autoCloseDelay = 4f;
        [SerializeField] private int maxPanelsPerLane = 5;

        private List<UI.InteractionInfoPanel> leftPanels = new List<UI.InteractionInfoPanel>();
        private List<UI.InteractionInfoPanel> rightPanels = new List<UI.InteractionInfoPanel>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            CheckAutoClose(leftPanels);
            CheckAutoClose(rightPanels);
        }

        public void ShowPanel(IInteractable target, PanelLane lane)
        {
            if (target == null || panelPrefab == null) return;

            List<UI.InteractionInfoPanel> targetList = lane == PanelLane.Left ? leftPanels : rightPanels;
            Transform targetContainer = lane == PanelLane.Left ? leftLaneContainer : rightLaneContainer;

            UI.InteractionInfoPanel existingPanel = FindPanelByTarget(target, targetList);
            if (existingPanel != null)
            {
                existingPanel.Refresh();
                existingPanel.OnTouched();
                return;
            }

            if (targetList.Count >= maxPanelsPerLane)
            {
                UI.InteractionInfoPanel oldest = targetList[0];
                targetList.RemoveAt(0);
                if (oldest != null) Destroy(oldest.gameObject);
            }

            UI.InteractionInfoPanel newPanel = Instantiate(panelPrefab, targetContainer);
            newPanel.Initialize(target);
            targetList.Add(newPanel);
        }

        public void RemovePanel(UI.InteractionInfoPanel panel)
        {
            if (panel == null) return;

            if (leftPanels.Remove(panel))
            {
                Destroy(panel.gameObject);
                return;
            }

            if (rightPanels.Remove(panel))
            {
                Destroy(panel.gameObject);
            }
        }

        private UI.InteractionInfoPanel FindPanelByTarget(IInteractable target, List<UI.InteractionInfoPanel> panels)
        {
            GameObject targetGameObject = (target as MonoBehaviour)?.gameObject;
            if (targetGameObject == null) return null;

            foreach (UI.InteractionInfoPanel panel in panels)
            {
                if (panel == null) continue;

                GameObject panelTargetGameObject = (panel.Target as MonoBehaviour)?.gameObject;
                if (panelTargetGameObject == targetGameObject)
                {
                    return panel;
                }
            }

            return null;
        }

        private void CheckAutoClose(List<UI.InteractionInfoPanel> panels)
        {
            for (int i = panels.Count - 1; i >= 0; i--)
            {
                UI.InteractionInfoPanel panel = panels[i];

                if (panel.TimeSinceLastTouch >= autoCloseDelay)
                {
                    if (IsMouseOverPanel(panel))
                    {
                        continue;
                    }

                    panels.RemoveAt(i);
                    Destroy(panel.gameObject);
                }
            }
        }

        private bool IsMouseOverPanel(UI.InteractionInfoPanel panel)
        {
            if (panel == null) return false;

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null) return false;

            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                null
            );
        }
    }
}
