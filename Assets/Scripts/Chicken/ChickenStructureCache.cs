using System.Collections.Generic;
using UnityEngine;
using GallinasFelices.Structures;

namespace GallinasFelices.Chicken
{
    public class ChickenStructureCache : MonoBehaviour
    {
        private static List<Feeder> feeders = new List<Feeder>();
        private static List<WaterTrough> waterTroughs = new List<WaterTrough>();
        private static List<Coop> coops = new List<Coop>();

        public static void RegisterFeeder(Feeder feeder)
        {
            if (!feeders.Contains(feeder))
            {
                feeders.Add(feeder);
            }
        }

        public static void UnregisterFeeder(Feeder feeder)
        {
            feeders.Remove(feeder);
        }

        public static void RegisterWaterTrough(WaterTrough trough)
        {
            if (!waterTroughs.Contains(trough))
            {
                waterTroughs.Add(trough);
            }
        }

        public static void UnregisterWaterTrough(WaterTrough trough)
        {
            waterTroughs.Remove(trough);
        }

        public static void RegisterCoop(Coop coop)
        {
            if (!coops.Contains(coop))
            {
                coops.Add(coop);
            }
        }

        public static void UnregisterCoop(Coop coop)
        {
            coops.Remove(coop);
        }

        public static Feeder FindClosestAvailableFeeder(Vector3 position)
        {
            Feeder closest = null;
            float closestDistance = float.MaxValue;

            foreach (var feeder in feeders)
            {
                if (feeder == null || feeder.IsEmpty)
                {
                    continue;
                }

                StructureDurability durability = feeder.GetComponent<StructureDurability>();
                if (durability != null && durability.IsBroken)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, feeder.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = feeder;
                }
            }

            return closest;
        }

        public static WaterTrough FindClosestAvailableWaterTrough(Vector3 position)
        {
            WaterTrough closest = null;
            float closestDistance = float.MaxValue;

            foreach (var trough in waterTroughs)
            {
                if (trough == null || trough.IsEmpty)
                {
                    continue;
                }

                StructureDurability durability = trough.GetComponent<StructureDurability>();
                if (durability != null && durability.IsBroken)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, trough.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = trough;
                }
            }

            return closest;
        }

        public static Coop FindClosestAvailableCoop(Vector3 position)
        {
            Coop closest = null;
            float closestDistance = float.MaxValue;

            foreach (var coop in coops)
            {
                if (coop == null || coop.IsFull)
                {
                    continue;
                }

                StructureDurability durability = coop.GetComponent<StructureDurability>();
                if (durability != null && durability.IsBroken)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, coop.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = coop;
                }
            }

            return closest;
        }

        public static void ClearAll()
        {
            feeders.Clear();
            waterTroughs.Clear();
            coops.Clear();
        }
    }
}
