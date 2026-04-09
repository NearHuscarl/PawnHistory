using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class ThingBuilder(ThingDef def, ThingDef stuffDef = null)
{
    private readonly ThingDef def = def;
    private Map map = Find.CurrentMap;
    private readonly List<Action<Thing>> processors = [];

    private IntVec3? position;
    private int stackCount = 1;
    private ThingDef stuff = stuffDef;
    private Faction faction;
    private ThingPlaceMode placeMode = ThingPlaceMode.Near;

    public ThingBuilder At(IntVec3 pos)
    {
        position = pos;
        return this;
    }

    public ThingBuilder Stack(int count)
    {
        stackCount = count;
        return this;
    }

    public ThingBuilder MadeOf(ThingDef stuffDef)
    {
        stuff = stuffDef;
        return this;
    }

    public ThingBuilder Map(Map map)
    {
        this.map = map;
        return this;
    }

    public ThingBuilder Faction(Faction faction)
    {
        this.faction = faction;
        return this;
    }

    public ThingBuilder PlaceMode(ThingPlaceMode placeMode)
    {
        this.placeMode = placeMode;
        return this;
    }

    private IntVec3 ResolvePosition()
    {
        if (position.HasValue)
            return position.Value;

        return map.Center;
    }

    public void Do(Action<Thing> action)
    {
        processors.Add(action);
    }

    /// <summary>
    /// Creates and spawns the thing.
    /// </summary>
    public T Create<T>() where T : Thing
    {
        var thing = ThingMaker.MakeThing(def, stuff);
        
        thing.stackCount = stackCount;

        if (faction != null)
            thing.SetFaction(faction);

        foreach (var processor in processors)
            processor(thing);

        var cell = ResolvePosition();
        GenPlace.TryPlaceThing(thing, cell, map, placeMode);

        return thing as T;
    }
    public Thing Create() => Create<Thing>();
}

public static class ThingBuilderExtensions
{
    public static ThingBuilder PoisonFood(this ThingBuilder builder, Pawn cook)
    {
        builder.Do(thing =>
        {
            thing.TryGetComp<CompCookTracker>()?.cook = cook;
            thing.TryGetComp<CompFoodPoisonable>()?.SetPoisoned(FoodPoisonCause.IncompetentCook);
        });

        return builder;
    }
}