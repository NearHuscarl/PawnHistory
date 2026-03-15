using System;

namespace PawnHistory.Source.DebugTools;

/// <summary>
/// https://github.com/pardeike/Rimworld-Doorstop
/// </summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
public class ReloadableAttribute : Attribute { }
