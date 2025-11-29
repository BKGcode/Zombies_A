using UnityEngine;
using System.Collections.Generic;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "ChickenConfig", menuName = "Gallinas Felices/Chicken Config")]
    public class ChickenConfigSO : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseChickenLifespanMinutes = 30f;
        public float baseEggProductionTime = 15f;

        [Header("UI Texts")]
        public string ageLabel = "Edad";
        public string happinessLabel = "Felicidad";

        [Header("Chicken Names")]
        [Tooltip("Lista de nombres para asignar aleatoriamente a las gallinas")]
        public string[] chickenNames = new string[]
        {
            "Clucky", "Nugget", "Drumstick", "Peep", "Scrambles",
            "Sunny", "Eggbert", "Feathers", "Pecky", "Henrietta",
            "Colonel", "Wingnut", "Rooster", "Pickles", "Biscuit",
            "Waffles", "Popcorn", "Pepper", "Fluffy", "Chickie"
        };

        [Header("State Texts")]
        public List<ChickenStateText> stateTexts = new List<ChickenStateText>();

        [System.Serializable]
        public struct ChickenStateText
        {
            public Chicken.ChickenState state;
            public string text;
        }

        public string GetStateText(Chicken.ChickenState state)
        {
            foreach (var item in stateTexts)
            {
                if (item.state == state) return item.text;
            }
            return state.ToString();
        }

        public string GetRandomName()
        {
            if (chickenNames == null || chickenNames.Length == 0)
            {
                return "Chicken";
            }
            return chickenNames[Random.Range(0, chickenNames.Length)];
        }

        private void OnValidate()
        {
            // Populate default values if empty
            if (stateTexts == null || stateTexts.Count == 0)
            {
                stateTexts = new List<ChickenStateText>()
                {
                    new ChickenStateText { state = Chicken.ChickenState.Idle, text = "Observando el mundo" },
                    new ChickenStateText { state = Chicken.ChickenState.Walking, text = "Paseando" },
                    new ChickenStateText { state = Chicken.ChickenState.Eating, text = "Picoteando" },
                    new ChickenStateText { state = Chicken.ChickenState.Drinking, text = "Bebiendo agua" },
                    new ChickenStateText { state = Chicken.ChickenState.Sleeping, text = "Durmiendo" },
                    new ChickenStateText { state = Chicken.ChickenState.GoingToNest, text = "Buscando nido" },
                    new ChickenStateText { state = Chicken.ChickenState.LayingEgg, text = "Poniendo un huevito" },
                    new ChickenStateText { state = Chicken.ChickenState.Exploring, text = "Explorando" },
                    new ChickenStateText { state = Chicken.ChickenState.DustBathing, text = "Revolcándose" }
                };
            }
        }
    }
}
