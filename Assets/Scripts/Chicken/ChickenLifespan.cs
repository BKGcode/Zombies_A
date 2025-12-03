using UnityEngine;
using System;
using GallinasFelices.Data;

namespace GallinasFelices.Chicken
{
    [RequireComponent(typeof(Chicken))]
    public class ChickenLifespan : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameBalanceSO gameBalance;

        public event Action<Chicken> OnChickenDied;

        private Chicken chicken;
        private ChickenNeeds needs;
        private float ageTimer;
        private float neglectTimer;
        private float maxLifespanSeconds;

        private void Awake()
        {
            chicken = GetComponent<Chicken>();
        }

        private void Start()
        {
            if (chicken != null)
            {
                needs = chicken.Needs;
            }

            if (gameBalance == null)
            {
                Debug.LogError("GameBalanceSO not assigned in ChickenLifespan!");
                enabled = false;
                return;
            }

            // Calculate max lifespan based on base value and personality modifier
            float baseMinutes = 30f; // Default fallback
            if (chicken.ChickenConfig != null)
            {
                baseMinutes = chicken.ChickenConfig.baseChickenLifespanMinutes;
            }
            
            float baseSeconds = baseMinutes * 60f;
            float modifier = chicken.Personality != null ? chicken.Personality.lifespanModifier : 1f;
            maxLifespanSeconds = baseSeconds * modifier;
        }

        private void Update()
        {
            UpdateAge();
            UpdateNeglect();
        }

        private void UpdateAge()
        {
            if (chicken != null && chicken.VisualRoot != null && !chicken.VisualRoot.gameObject.activeSelf)
            {
                return;
            }

            ageTimer += Time.deltaTime;

            if (ageTimer >= maxLifespanSeconds)
            {
                Die("Old Age");
            }
        }

        private void UpdateNeglect()
        {
            if (chicken != null && chicken.VisualRoot != null && !chicken.VisualRoot.gameObject.activeSelf)
            {
                neglectTimer = 0f;
                return;
            }

            bool isStarving = needs.Hunger >= 100f;
            bool isDehydrated = needs.Thirst >= 100f;

            if (isStarving || isDehydrated)
            {
                neglectTimer += Time.deltaTime;
                if (neglectTimer >= gameBalance.neglectDeathTimeSeconds)
                {
                    Die("Neglect (Starvation/Dehydration)");
                }
            }
            else
            {
                neglectTimer = 0f;
            }
        }

        private void Die(string cause)
        {
            Debug.Log($"🐔 Chicken '{name}' died of: {cause}");
            OnChickenDied?.Invoke(chicken);
            
            // Visual feedback could be added here (particle, animation trigger)
            
            Destroy(gameObject);
        }

        // Public getters for UI or Debugging
        public float CurrentAge => ageTimer;
        public float MaxLifespan => maxLifespanSeconds;
        public float NeglectTimer => neglectTimer;
    }
}
