using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class IdeoBuilder
{
    private readonly List<Action<Ideo>> processors = [];

    private readonly Dictionary<PreceptDef, List<PreceptDef>> implicitPreceptLookup = new()
    {
        { PreceptDefOf.Funeral, [PreceptDefOf.FuneralNoCorpse] }
    };
    
    public IdeoBuilder AddPrecept(PreceptDef preceptDef, RitualPatternDef fillWith = null)
    {
        processors.Add(ideo =>
        {
            var precept = PreceptMaker.MakePrecept(preceptDef);
            ideo.AddPrecept(precept, init: true, fillWith: fillWith ?? preceptDef.ritualPatternBase);

            // remove missing hidden Precept warning
            if (implicitPreceptLookup.TryGetValue(preceptDef, out var implicitPrecepts))
            {
                foreach (var p in implicitPrecepts)
                {
                    var precept2 = PreceptMaker.MakePrecept(p);
                    ideo.AddPrecept(precept2, init: true, fillWith: p.ritualPatternBase);
                }
            }
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
