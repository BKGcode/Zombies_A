using UnityEngine;

namespace GallinasFelices.Core
{
    public interface IInteractable
    {
        string GetTitle();
        string GetMainInfo();
        InteractionButton[] GetActions();
        InteractionBar[] GetBars();
    }

    public struct InteractionButton
    {
        public string label;
        public bool isEnabled;
        public System.Action onClick;

        public InteractionButton(string label, bool isEnabled, System.Action onClick)
        {
            this.label = label;
            this.isEnabled = isEnabled;
            this.onClick = onClick;
        }
    }

    public struct InteractionBar
    {
        public string label;
        public float value;
        public float maxValue;

        public InteractionBar(string label, float value, float maxValue)
        {
            this.label = label;
            this.value = value;
            this.maxValue = maxValue;
        }
    }
}
