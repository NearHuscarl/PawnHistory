# DefLookup To Extra Rename

## Summary

- Renamed the fallback def container from `DefLookup` to `Extra`.
- Renamed every nested static class to the `XyzDefOf` form so fallback lookups read as `Extra.ThingDefOf.SomeDef`.
- Updated recorder code, test helpers, and markdown guidance to use the new naming.

## Verification

- `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
  - Result: success, 0 warnings, 0 errors
