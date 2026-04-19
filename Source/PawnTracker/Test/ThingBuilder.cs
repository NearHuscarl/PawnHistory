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

    public ThingBuilder Map(Map map1)
    {
        this.map = map1;
        return this;
    }

    public ThingBuilder Faction(Faction faction1)
    {
        this.faction = faction1;
        return this;
    }

    public ThingBuilder PlaceMode(ThingPlaceMode placeMode1)
    {
        this.placeMode = placeMode1;
        return this;
    }

    public ThingBuilder Quality(QualityCategory quality)
    {
        return Do(thing =>
        {
            if (thing.TryGetComp<CompQuality>(out var comp))
                comp.SetQuality(quality, null);
        });
    }

    private IntVec3 ResolvePosition()
    {
        if (position.HasValue)
            return position.Value;

        return map.Center;
    }

    public ThingBuilder Do(Action<Thing> action)
    {
        processors.Add(action);
        return this;
    }

    private T CreateInternal<T>() where T : Thing
    {
        var thing = ThingMaker.MakeThing(def, stuff);
        
        thing.stackCount = stackCount;

        if (faction != null)
            thing.SetFaction(faction);

        foreach (var processor in processors)
            processor(thing);

        return thing as T;
    }
    
    public T CreateAndPutInto<T>(Pawn pawn) where T : Thing
    {
        var thing = CreateInternal<T>();
        pawn.inventory.innerContainer.TryAdd(thing);

        return thing;
    }
    public Thing CreateAndPutInto(Pawn pawn) => CreateAndPutInto<Thing>(pawn);

    /// <summary>
    /// Creates and spawns the thing.
    /// </summary>
    public T Create<T>() where T : Thing
    {
        var thing = CreateInternal<T>();
        var cell = ResolvePosition();
        GenPlace.TryPlaceThing(thing, cell, map, placeMode);

        return thing;
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