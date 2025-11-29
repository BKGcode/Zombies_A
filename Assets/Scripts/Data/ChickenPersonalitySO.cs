using UnityEngine;
using GallinasFelices.Chicken;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "ChickenPersonality", menuName = "Gallinas Felices/Chicken Personality")]
    public class ChickenPersonalitySO : ScriptableObject
    {
        [Header("Personality Identity")]
        public ChickenPersonalityType personalityType;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Behavior Modifiers")]
        [Range(0.5f, 2f)]
        public float walkSpeedMultiplier = 1f;

        [Range(0.5f, 2f)]
        public float idleTimeMultiplier = 1f;

        [Range(0f, 1f)]
        public float sociabilityLevel = 0.5f;

        [Range(0f, 1f)]
        public float curiosityLevel = 0.5f;

        [Header("Production Modifiers")]
        public float minEggProductionTime = 15f;
        public float maxEggProductionTime = 20f;

        [Range(0.8f, 1.2f)]
        public float happinessMultiplier = 1f;

        [Header("State Preferences")]
        [Range(0f, 1f)]
        public float exploreChance = 0.3f;

        [Range(0f, 1f)]
        public float dustBathChance = 0.2f;

        [Header("Movement Settings")]
        public float wanderRadius = 10f;
        public float minWanderTime = 2f;
        public float maxWanderTime = 5f;

        [Header("Idle Fidgeting")]
        [Tooltip("Maximum rotation angle when fidgeting")]
        public float maxFidgetAngle = 30f;
        [Tooltip("Speed of rotation when fidgeting")]
        public float fidgetRotationSpeed = 20f;
        [Tooltip("Time between fidget direction changes")]
        public float minFidgetChangeTime = 1f;
        public float maxFidgetChangeTime = 3f;

        [Header("State Durations")]
        public float minIdleTime = 1f;
        public float maxIdleTime = 4f;
        public float minWaitAfterWalk = 2f;
        public float maxWaitAfterWalk = 6f;
        public float minEatingDuration = 4f;
        public float maxEatingDuration = 8f;
        public float minDrinkingDuration = 2f;
        public float maxDrinkingDuration = 4f;
        public float minLayingEggDuration = 4f;
        public float maxLayingEggDuration = 7f;

        [Header("Lifespan Modifier")]
        [Tooltip("Multiplier for base lifespan. Longevous = 1.5, Normal = 1.0, Fragile = 0.7")]
        [Range(0.5f, 2f)]
        public float lifespanModifier = 1f;

        [Header("Locomotion Personality")]
        [Tooltip("Pause probability during navigation (0=never, 0.5=very frequent)")]
        [Range(0f, 0.5f)]
        public float pauseFrequency = 0.1f;

        [Tooltip("Speed variation ±X% (0.2 = ±20%)")]
        [Range(0f, 0.5f)]
        public float speedVariation = 0.2f;

        [Tooltip("Avoidance priority (0=always evades, 99=never evades)")]
        [Range(0, 99)]
        public int avoidancePriority = 50;
    }
}
