using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Gallinas Felices/Game Balance")]
    public class GameBalanceSO : ScriptableObject
    {
        [Header("Egg Production")]
        public int eggValue = 1;

        [Header("Happiness System")]
        public float happinessDecayRate = 0.1f;
        public float happinessFromFood = 10f;
        public float happinessFromWater = 10f;
        public float happinessFromShadow = 5f;
        public float happinessFromSleep = 15f;

        [Header("Needs System")]
        public float hungerIncreaseRate = 1f;
        public float thirstIncreaseRate = 1.2f;
        public float energyDecreaseRate = 0.8f;

        [Header("Structure Consumption")]
        [Tooltip("Amount consumed per use action")]
        public float consumptionPerUse = 5f;

        [Header("Economy")]
        public int chickenBaseCost = 50;
        public int feederUpgradeCost = 30;
        public int autoCollectorCost = 100;

        [Header("Structure Repair Costs")]
        public int feederRepairCost = 20;
        public int waterTroughRepairCost = 20;
        public int nestRepairCost = 15;
        public int coopRepairCost = 50;

        [Header("Resource Refill Costs")]
        public int feederRefillCost = 10;
        public int waterTroughRefillCost = 10;

        [Header("Durability Settings")]
        [Tooltip("Percentage decay per minute from time")]
        public float structureTimeDecayRate = 1f;
        [Tooltip("Percentage decay per use action")]
        public float structureUseDecayAmount = 0.1f;



        [Header("Auto-Manager Thresholds")]
        [Tooltip("Durability % threshold to trigger auto-repair")]
        public float autoRepairThreshold = 30f;
        [Tooltip("Capacity % threshold to trigger auto-refill")]
        public float autoRefillThreshold = 20f;

        [Header("Coop Settings")]
        [Tooltip("Range for coop detection")]
        public float coopRange = 3f;

        [Header("Egg Settings")]
        public float eggAutoCollectTime = 10f;

        [Header("Time Settings")]
        public float secondsPerGameHour = 60f;
        public float startHour = 6f;

        [Header("Time Of Day Thresholds")]
        public float morningStart = 6f;
        public float dayStart = 10f;
        public float afternoonStart = 16f;
        public float nightStart = 20f;

        [Header("Lighting Settings")]
        public float nightIntensity = 0.1f;
        public float dayIntensity = 1f;

        [Header("Structure Ranges")]
        public float feedingRange = 2f;
        public float drinkingRange = 2f;

        [Header("Chicken Lifespan")]
        public float neglectDeathTimeSeconds = 120f;

        [Header("Day/Night Cycle System")]
        [Tooltip("Ratio día:noche (ej: 3 = día dura 3x más que noche)")]
        [Range(1f, 10f)]
        public float dayNightRatio = 3f;
        
        [Header("Light Color Gradient")]
        [Tooltip("Gradient de colores para la luz direccional a lo largo del día")]
        public Gradient lightColorGradient;
        
        [Header("Skybox Materials")]
        [Tooltip("Material de skybox para el día")]
        public Material daySkybox;
        [Tooltip("Material de skybox para la noche")]
        public Material nightSkybox;
        
        [Header("Nap System")]
        [Tooltip("Umbral de energía para que las gallinas tomen siestas durante el día")]
        [Range(0f, 100f)]
        public float napEnergyThreshold = 50f;
        
        [Header("Sleep Location Penalties")]
        [Tooltip("Penalización de lifespan por dormir fuera del coop (porcentaje por noche)")]
        [Range(0f, 0.2f)]
        public float sleepOutsideLifespanPenalty = 0.05f;
        
        [Tooltip("Multiplicador de pérdida de energía al dormir fuera (1.0 = normal, 2.0 = doble)")]
        [Range(1f, 5f)]
        public float sleepOutsideEnergyPenalty = 2f;
        
        [Header("Coop Destroyed Settings")]
        [Tooltip("Radio de dispersión cuando se destruye un coop")]
        public float coopDestroyedScatterRadius = 8f;
        
        [Tooltip("Fuerza del scatter (para teleport offset)")]
        [Range(0.5f, 2f)]
        public float coopDestroyedScatterForce = 1f;
    }
}
