using Verse;

namespace PawnHistory.Source.Helper;

public static class BodyPartRecordHelper
{
    extension(BodyPartRecord bodyPartRecord)
    {
        // True when this record is one instance of a repeated body part,
        // e.g. left lung, right arm, left little finger.
        public bool IsOneOfMultipleParts => !bodyPartRecord.customLabel.NullOrEmpty();
    }
}