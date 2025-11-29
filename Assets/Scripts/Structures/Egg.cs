using UnityEngine;
using UnityEngine.Events;
using GallinasFelices.Data;

namespace GallinasFelices.Structures
{
    public class Egg : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameBalanceSO gameBalance;

        [Header("Events")]
        public UnityEvent<int> OnCollected = new UnityEvent<int>();

        private Nest parentNest;
        private float timer = 0f;

        public void SetNest(Nest nest)
        {
            parentNest = nest;
        }

        public GameBalanceSO GameBalance
        {
            get => gameBalance;
            set => gameBalance = value;
        }

        private void Start()
        {
            if (gameBalance == null)
            {
                // Fallback: Try to find FarmManager and get it from there
                var farmManager = FindObjectOfType<GallinasFelices.Core.FarmManager>();
                /* 
                   Note: FarmManager doesn't expose GameBalanceSO publicly yet. 
                   We should probably load it from Resources if it's a singleton asset, 
                   or just ensure the Prefab has it assigned.
                */
                
                // Better fallback: Load from Resources if we have a standard path, 
                // or just log a clearer error.
                Debug.LogWarning($"[Egg] GameBalanceSO not assigned on {gameObject.name}! Auto-collect will not work.");
            }
        }

        private void Update()
        {
            if (gameBalance == null) return;

            timer += Time.deltaTime;
            if (timer >= gameBalance.eggAutoCollectTime)
            {
                Debug.Log("[Egg] Auto-collecting");
                Collect();
            }
        }

        public void Collect()
        {
            int value = gameBalance != null ? gameBalance.eggValue : 1;
            Debug.Log($"[Egg] Collect() called - Value: {value}");
            OnCollected?.Invoke(value);

            if (parentNest != null)
            {
                parentNest.RemoveEgg();
            }

            Destroy(gameObject);
        }

        private void OnMouseDown()
        {
            Collect();
        }
    }
}
