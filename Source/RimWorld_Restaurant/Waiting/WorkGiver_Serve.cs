using System.Collections.Generic;
using System.Linq;
using Restaurant.Dining;
using RimWorld;
using Verse;
using Verse.AI;

namespace Restaurant.Waiting
{
    public class WorkGiver_Serve : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.GetRestaurant().SpawnedDiningPawns;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.GetRestaurant().AvailableOrdersForServing.Any();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn patron)) return false;

            if (pawn == t) return false;

            var driver = patron.GetDriver<JobDriver_Dine>();
            if (driver == null || driver.wantsToOrder) return false;

            var restaurant = pawn.GetRestaurant();
            var order = restaurant.GetOrderFor(patron);

            if (order == null) return false;
            if (order.delivered) return false;

            if (restaurant.IsBeingDelivered(order, patron)) return false;

            if (!patron.Spawned || patron.Dead)
            {
                Log.Message($"Order canceled. null? {order.patron == null} dead? {order.patron.Dead} unspawned? {!order.patron?.Spawned}");
                restaurant.CancelOrder(order);
                return false;
            }

            Log.Message($"{pawn.NameShortColored} is trying to serve {patron.NameShortColored} a {order.consumableDef.label}.");
            var consumable = restaurant.GetServableThing(order, pawn);

            if (consumable == null)
            {
                Log.Message($"Nothing found that matches order.");
                return false;
            }

            if (RestaurantUtility.IsRegionDangerous(pawn, patron.GetRegion()) && !forced) return false;
            if (RestaurantUtility.IsRegionDangerous(pawn, consumable.GetRegion()) && !forced) return false;

            Log.Message($"{pawn.NameShortColored} can serve {consumable.Label} to {order.patron.NameShortColored}.");
            order.consumable = consumable; // Store for JobOnThing
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn p)) return null;

            var order = pawn.GetRestaurant().GetOrderFor(p);
            var consumable = order.consumable;
            if(consumable == null) Log.Error($"Consumable in order for {p.NameShortColored} is suddenly null.");

            return JobMaker.MakeJob(WaitingUtility.serveDef, order.patron, consumable);
        }
    }
}
