using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "CoopConfig", menuName = "Gallinas Felices/Structure Configs/Coop Config")]
    public class CoopConfigSO : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("Nivel de la estructura (1, 2, 3)")]
        public int level = 1;
        
        [Tooltip("Nombre del nivel (Básico, Mejorado, Avanzado)")]
        public string levelName = "Básico";
        
        [Header("Capacity")]
        [Tooltip("Número de puntos de sueño disponibles")]
        [Range(5, 50)]
        public int sleepingSpots = 15;
        
        [Header("Upgrade")]
        [Tooltip("Coste en huevos para mejorar a siguiente nivel")]
        public int upgradeCost = 200;
        
        [Tooltip("Referencia al siguiente nivel (null si es máximo)")]
        public CoopConfigSO nextLevel;
        
        [Header("Visual (Optional)")]
        [Tooltip("Material para este nivel (opcional)")]
        public Material material;
        
        public bool CanUpgrade => nextLevel != null;
    }
}
