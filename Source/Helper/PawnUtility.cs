using PawnHistory.Source.PawnTracker;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.Helper;

internal static class PawnUtility
{
    extension(Pawn pawn)
    {
        // GrammarResolverSimple.cs -> "nameDef"
        // TODO: test with shambler, shambler past colonist..
        public string NameDef =>
            pawn.Name != null
                ? Find.ActiveLanguageWorker.WithDefiniteArticlePostProcessed(pawn.Name.ToStringShort, pawn.gender, name: true).ApplyTag(TagType.Name).Resolve()
                : pawn.KindLabelDefinite().ApplyTag(TagType.Name).Resolve();

        public List<HistoryRecord> HistoryRecords => CompHistoryManager.GetComp(pawn)?.records ?? [];
        public List<HistoryRecord> VisibleHistoryRecords => pawn.HistoryRecords.Where(r => r.def.importance != RecordImportance.Debug).ToList();

        public bool IsFactionLeader(Faction faction = null)
        {
            return (faction ?? pawn.Faction)?.leader == pawn;
        }

        public void MakeDowned()
        {
            pawn.health.forceDowned = true;
            Accessor.Pawn_HealthTracker.MakeDowned(pawn.health, null, null);
            pawn.health.forceDowned = false;
        }

        public int GetTileId()
        {
            return pawn.MapHeld?.Tile.tileId
                   ?? pawn.GetCaravan()?.Tile.tileId
                   ?? Find.AnyPlayerHomeMap?.Tile.tileId
                   ?? -1;
        }

        public bool IsHavingAffairBasedOnIdeo()
        {
            return !new HistoryEvent(pawn.GetHistoryEventLoveRelationCount(), pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo();
        }

        public void StartMentalBreakWithMadeUpThought(MentalBreakDef def)
        {
            var randomNegativeThought = DefDatabase<ThoughtDef>.AllDefs
                .Where(t => t.stages != null && t.stages.Any(s => s is { baseMoodEffect: < 0 }) && (!t.label.NullOrEmpty() || !t.stages.First().label.NullOrEmpty()))
                .RandomElementWithFallback();
            var reason = "MentalStateReason_Mood".Translate() + "\n\n" + "FinalStraw".Translate(randomNegativeThought.LabelCap);

            if (!pawn.mindState.mentalBreaker.TryDoMentalBreak(reason, def))
                L.Warning($"Failed to force mental break {def.defName} on {pawn.LabelShort}");
        }

        public Pawn GiveBirth(Pawn parent2)
        {
            pawn.gender = Gender.Female;
            parent2.gender = Gender.Male;
            Hediff_Pregnant.DoBirthSpawn(pawn, parent2);

            var pawns = pawn.Map.mapPawns.AllHumanlikeSpawned;
            for (var i = pawns.Count - 1; i >= 0; i--)
            {
                var candidate = pawns[i];
                if (candidate.DevelopmentalStage != DevelopmentalStage.Baby)
                    continue;
                
                if (candidate.relations.DirectRelationExists(PawnRelationDefOf.Parent, parent2))
                    return candidate;
            }

            return null;
        }

        private static float GetDangerScore(Hediff h)
        {
            if (h.def.lethalSeverity <= 0f)
                return h.Severity / 1f / 3; // not lethal

            return h.Severity / h.def.lethalSeverity;
        }
        
        public Hediff GetMostDangerousHediff(BodyPartRecord part)
        {
            return HediffHelper.GetVisibleHediffs(pawn).Where(h => h.Visible && h.Part == part && h.def.isBad).OrderByDescending(GetDangerScore).FirstOrDefault();
        }

        /// <summary>
        /// Copied from HealthCardUtility.DrawHediffListing()
        /// </summary>
        /// <returns></returns>
        public string GetBloodLossText()
        {
            var bloodLoss = HealthUtility.TicksUntilDeathDueToBloodLoss(pawn);

            if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless))
                return "(" + "Deathless".Translate() + ")";
            if (bloodLoss >= 60000)
                return "(" + "WontBleedOutSoon".Translate() + ")";
            return "(" + "TimeToDeath".Translate((NamedArgument)bloodLoss.ToStringTicksToPeriod()) + ")";
        }
    }
}
