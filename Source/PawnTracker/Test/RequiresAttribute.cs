using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiresAttribute(string modId) : Attribute
{
    public string ModId => modId;
    public string ModName => modId.Split(".").LastOrDefault();
    public bool IsActive => ModsConfig.IsActive(modId);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RequiresRoyaltyAttribute() : RequiresAttribute(ModContentPack.RoyaltyModPackageId);

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RequiresIdeologyAttribute() : RequiresAttribute(ModContentPack.IdeologyModPackageId);

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RequiresBiotechAttribute() : RequiresAttribute(ModContentPack.BiotechModPackageId);

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RequiresAnomalyAttribute() : RequiresAttribute(ModContentPack.AnomalyModPackageId);

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class RequiresOdysseyAttribute() : RequiresAttribute(ModContentPack.OdysseyModPackageId);
