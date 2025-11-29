using UnityEngine;

namespace GallinasFelices.Core
{
    public class EggCounter : MonoBehaviour
    {
        public static EggCounter Instance { get; private set; }

        [Header("Egg Counter")]
        [SerializeField] private int totalEggs = 0;

        public int TotalEggs => totalEggs;

        public event System.Action<int> OnEggCountChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void AddEggs(int amount)
        {
            totalEggs += amount;
            OnEggCountChanged?.Invoke(totalEggs);
        }

        public bool TrySpendEggs(int amount)
        {
            if (totalEggs >= amount)
            {
                totalEggs -= amount;
                OnEggCountChanged?.Invoke(totalEggs);
                return true;
            }

            return false;
        }

        public bool CanAfford(int cost)
        {
            return totalEggs >= cost;
        }
    }
}
