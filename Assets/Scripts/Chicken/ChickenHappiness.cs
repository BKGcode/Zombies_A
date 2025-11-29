using System;
using UnityEngine;

namespace GallinasFelices.Chicken
{
    [Serializable]
    public class ChickenHappiness
    {
        [SerializeField, Range(0f, 100f)] private float currentHappiness = 75f;

        [Header("Happiness Factors")]
        [SerializeField] private bool hasFood = true;
        [SerializeField] private bool hasWater = true;
        [SerializeField] private bool inShadow = false;
        [SerializeField] private bool isRested = true;

        public float CurrentHappiness => currentHappiness;

        public event Action<float> OnHappinessChanged;

        public void UpdateHappiness(float deltaTime, float decayRate)
        {
            float targetHappiness = CalculateTargetHappiness();
            currentHappiness = Mathf.Lerp(currentHappiness, targetHappiness, deltaTime * decayRate);
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, 100f);

            OnHappinessChanged?.Invoke(currentHappiness);
        }

        private float CalculateTargetHappiness()
        {
            float target = 50f;

            if (hasFood)
            {
                target += 15f;
            }

            if (hasWater)
            {
                target += 15f;
            }

            if (inShadow)
            {
                target += 10f;
            }

            if (isRested)
            {
                target += 10f;
            }

            return Mathf.Clamp(target, 0f, 100f);
        }

        public void SetHasFood(bool value)
        {
            hasFood = value;
        }

        public void SetHasWater(bool value)
        {
            hasWater = value;
        }

        public void SetInShadow(bool value)
        {
            inShadow = value;
        }

        public void SetIsRested(bool value)
        {
            isRested = value;
        }

        public void AddHappiness(float amount)
        {
            currentHappiness = Mathf.Clamp(currentHappiness + amount, 0f, 100f);
            OnHappinessChanged?.Invoke(currentHappiness);
        }

        public float GetProductionMultiplier()
        {
            return Mathf.Lerp(0.5f, 1.5f, currentHappiness / 100f);
        }
    }
}
