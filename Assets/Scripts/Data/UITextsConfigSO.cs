using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "UITextsConfig", menuName = "Gallinas Felices/UI Texts Config")]
    public class UITextsConfigSO : ScriptableObject
    {
        [Header("Chicken Warnings")]
        [Tooltip("Mensaje cuando la gallina tiene hambre")]
        public string hungerWarning = "¡Tiene hambre!";
        
        [Tooltip("Mensaje cuando la gallina tiene sed")]
        public string thirstWarning = "¡Tiene sed!";

        [Header("Chicken Bar Labels")]
        [Tooltip("Etiqueta para la barra de vida/edad")]
        public string lifeLabel = "Vida";
        
        [Tooltip("Etiqueta para la barra de felicidad")]
        public string happinessLabel = "Felicidad";

        [Header("Structure Titles")]
        [Tooltip("Título del nido")]
        public string nestTitle = "Nido";
        
        [Tooltip("Título de la caseta")]
        public string coopTitle = "Caseta";
        
        [Tooltip("Título del comedero")]
        public string feederTitle = "Comedero";
        
        [Tooltip("Título del bebedero")]
        public string waterTroughTitle = "Bebedero";

        [Header("Structure States")]
        [Tooltip("Estado cuando la estructura funciona")]
        public string operativeState = "Operativo";
        
        [Tooltip("Estado cuando la estructura está rota")]
        public string brokenState = "ROTO";

        [Header("Bar Labels")]
        [Tooltip("Etiqueta para capacidad de estructuras")]
        public string capacityLabel = "Capacidad";
        
        [Tooltip("Etiqueta para durabilidad")]
        public string durabilityLabel = "Durabilidad";
        
        [Tooltip("Etiqueta para ocupación de coops")]
        public string occupancyLabel = "Ocupación";
        
        [Tooltip("Prefijo para estado en paneles")]
        public string statePrefix = "Estado";

        [Header("Farm Limits Messages")]
        [Tooltip("Mensaje cuando faltan nidos")]
        public string needMoreNests = "Necesitas más nidos";
        
        [Tooltip("Mensaje cuando faltan comederos")]
        public string needMoreFeeders = "Necesitas más comederos";
        
        [Tooltip("Mensaje cuando faltan bebederos")]
        public string needMoreWaterTroughs = "Necesitas más bebederos";
        
        [Tooltip("Sufijo para indicar el máximo por estructura (ej: 'Máx 3 gallinas/nido')")]
        public string maxPrefix = "Máx";
        
        [Tooltip("Texto para 'gallinas'")]
        public string chickensText = "gallinas";
        
        [Tooltip("Texto para 'nido' (singular)")]
        public string nestSingular = "nido";
        
        [Tooltip("Texto para 'comedero' (singular)")]
        public string feederSingular = "comedero";
        
        [Tooltip("Texto para 'bebedero' (singular)")]
        public string waterTroughSingular = "bebedero";
    }
}
