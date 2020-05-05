using HarmonyLib;
using Verse;
using Verse.AI;

namespace Restaurant.TableTops
{
    internal static class _Harmony_AvoidGrid_Patch
    {
        /// <summary>
        /// Is a tabletop spawned? Remove DiningSpots
        /// </summary>
        [HarmonyPatch(typeof(AvoidGrid), "Notify_BuildingSpawned")]
        public class Notify_BuildingSpawned
        {
            [HarmonyPostfix]
            internal static void Postfix(Building building, AvoidGrid __instance)
            {
                RegisterUtility.OnBuildingSpawned(building, __instance.map);
            }
        }

        /// <summary>
        /// Is a building removed? Remove tabletops and blueprints of tabletops at its location
        /// </summary>
        [HarmonyPatch(typeof(AvoidGrid), "Notify_BuildingDespawned")]
        public class Notify_BuildingDespawned
        {
            [HarmonyPostfix]
            internal static void Postfix(Building building, AvoidGrid __instance)
            {
                RegisterUtility.OnBuildingDespawned(building, __instance.map);
            }
        }
    }
}
