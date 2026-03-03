using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    public class CompProperties_History : CompProperties
    {
        public CompProperties_History() => compClass = typeof(CompHistory);
    }
}
