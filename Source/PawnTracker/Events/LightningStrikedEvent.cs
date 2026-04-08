using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record LightningStrikedEvent(IntVec3 StrikeLoc, Map Map, float Radius) : GameEventBase;

[HarmonyPatch(typeof(WeatherEvent_LightningStrike), nameof(WeatherEvent_LightningStrike.DoStrike))]
public static class WeatherEvent_LightningStrike_DoStrike_Patch
{
    // fucking tynan didn't bother to parameterize this
    public static readonly float StrikeRadius = 1.9f;

    public static void Postfix(IntVec3 strikeLoc, Map map, ref Mesh boltMesh)
    {
        GameEventBus.Publish(new LightningStrikedEvent(strikeLoc, map, StrikeRadius));
    }
}
