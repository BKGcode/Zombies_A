using UnityEngine;
using GallinasFelices.Core;

namespace GallinasFelices.Structures
{
    public class EggCollector : MonoBehaviour
    {
        private void OnEnable()
        {
            SubscribeToExistingEggs();
        }

        private void SubscribeToExistingEggs()
        {
            Egg[] eggs = FindObjectsOfType<Egg>();
            foreach (var egg in eggs)
            {
                SubscribeToEgg(egg);
            }
        }

        public void SubscribeToEgg(Egg egg)
        {
            if (egg != null)
            {
                egg.OnCollected.RemoveListener(OnEggCollected);
                egg.OnCollected.AddListener(OnEggCollected);
                Debug.Log($"[EggCollector] Subscribed to egg: {egg.gameObject.name}");
            }
        }

        private void OnEggCollected(int value)
        {
            Debug.Log($"[EggCollector] OnEggCollected called with value: {value}");
            if (EggCounter.Instance != null)
            {
                EggCounter.Instance.AddEggs(value);
                Debug.Log($"[EggCollector] Eggs added to counter. New total: {EggCounter.Instance.TotalEggs}");
            }
            else
            {
                Debug.LogError("[EggCollector] EggCounter.Instance is null!");
            }
        }
    }
}
