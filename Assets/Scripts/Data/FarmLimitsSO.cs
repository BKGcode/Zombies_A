using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "FarmLimits", menuName = "Gallinas Felices/Farm Limits")]
    public class FarmLimitsSO : ScriptableObject
    {
        [Header("Population Limits")]
        [Tooltip("Maximum chickens allowed per nest in the scene (ONLY nests limit total population)")]
        public int chickensPerNest = 5;
        
        [Header("Structure Build Limits")]
        [Tooltip("Maximum number of nests that can be built")]
        public int maxNests = 10;
        [Tooltip("Maximum number of feeders that can be built")]
        public int maxFeeders = 5;
        [Tooltip("Maximum number of water troughs that can be built")]
        public int maxWaterTroughs = 5;
    }
}
