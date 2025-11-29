using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "WaterTroughConfig", menuName = "Gallinas Felices/Structure Configs/Water Trough Config")]
    public class WaterTroughConfigSO : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("Nivel de la estructura (1, 2, 3)")]
        public int level = 1;
        
        [Tooltip("Nombre del nivel (Básico, Mejorado, Avanzado)")]
        public string levelName = "Básico";
        
        [Header("Capacity")]
        [Tooltip("Capacidad máxima de agua")]
        [Range(50f, 500f)]
        public float capacity = 100f;
        
        [Header("Usage Limits")]
        [Tooltip("Máximo de gallinas que pueden beber simultáneamente")]
        [Range(1, 20)]
        public int simultaneousUsers = 5;
        
        [Header("Upgrade")]
        [Tooltip("Coste en huevos para mejorar a siguiente nivel")]
        public int upgradeCost = 100;
        
        [Tooltip("Referencia al siguiente nivel (null si es máximo)")]
        public WaterTroughConfigSO nextLevel;
        
        [Header("Visual (Optional)")]
        [Tooltip("Material para este nivel (opcional)")]
        public Material material;
        
        public bool CanUpgrade => nextLevel != null;
    }
}
