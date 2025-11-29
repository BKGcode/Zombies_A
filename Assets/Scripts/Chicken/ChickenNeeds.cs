using System;
using UnityEngine;

namespace GallinasFelices.Chicken
{
    [Serializable]
    public class ChickenNeeds
    {
        [Header("Current Needs")]
        [SerializeField, Range(0f, 100f)] private float hunger = 50f;
        [SerializeField, Range(0f, 100f)] private float thirst = 50f;
        [SerializeField, Range(0f, 100f)] private float energy = 100f;

        public float Hunger => hunger;
        public float Thirst => thirst;
        public float Energy => energy;

        public void IncreaseHunger(float amount)
        {
            hunger = Mathf.Clamp(hunger + amount, 0f, 100f);
        }

        public void IncreaseThirst(float amount)
        {
            thirst = Mathf.Clamp(thirst + amount, 0f, 100f);
        }

        public void DecreaseEnergy(float amount)
        {
            energy = Mathf.Clamp(energy - amount, 0f, 100f);
        }

        public void Feed(float amount)
        {
            hunger = Mathf.Clamp(hunger - amount, 0f, 100f);
        }

        public void GiveWater(float amount)
        {
            thirst = Mathf.Clamp(thirst - amount, 0f, 100f);
        }

        public void RestoreEnergy(float amount)
        {
            energy = Mathf.Clamp(energy + amount, 0f, 100f);
        }

        public bool IsHungry()
        {
            return hunger > 70f;
        }

        public bool IsThirsty()
        {
            return thirst > 70f;
        }

        public bool IsTired()
        {
            return energy < 30f;
        }
    }
}
