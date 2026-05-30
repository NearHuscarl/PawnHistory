using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class IdeoBuilder
{
    private readonly List<Action<Ideo>> processors = [];
    
    public IdeoBuilder AddPrecept(PreceptDef preceptDef)
    {
        processors.Add(ideo =>
        {
            var precept = PreceptMaker.MakePrecept(preceptDef);
            ideo.AddPrecept(precept, init: true, fillWith: preceptDef.ritualPatternBase);
        });
        return this;
    }

    public Ideo Execute()
    {
        var ideo = IdeoGenerator.MakeFixedIdeo(new IdeoGenerationParms(Faction.OfPlayer.def, fixedIdeo: true));
        Find.IdeoManager.Add(ideo);
        processors.ForEach(p => p(ideo));
        return ideo;
    }
}
