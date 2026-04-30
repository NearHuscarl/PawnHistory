using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class ShuttleBuilder
{
    private readonly TransportShip transportShip;
    private readonly List<Thing> things = [];

    public ShuttleBuilder(TransportShip transportShip)
    {
        this.transportShip = transportShip;
    }

    public ShuttleBuilder Load(IEnumerable<Thing> thingsToLoad)
    {
        things.AddRange(thingsToLoad);
        return this;
    }

    public ShuttleBuilder Load(IEnumerable<Pawn> pawns) => Load(pawns.Cast<Thing>());
    public ShuttleBuilder Load(Thing thing) => Load([thing]);
    public ShuttleBuilder Load(Pawn pawn) => Load([pawn]);

    public void Launch(bool sendAway = true)
    {
        var transporter = transportShip.TransporterComp;

        foreach (var thing in things)
        {
            if (thing == null)
                continue;

            if (thing.Spawned)
                thing.DeSpawn();

            thing.holdingOwner?.Remove(thing);
            transporter.innerContainer.TryAddOrTransfer(thing);
        }

        if (!sendAway)
            return;

        SendTransportShipAwayUtility.SendTransportShipAway(transportShip, false, TransportShipDropMode.PawnsOnly);
    }
}
