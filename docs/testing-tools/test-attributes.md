# Test Attributes

Recorder tests use a small attribute set for discovery, gating, and parameterization.

## DLC Gating

`RequiresAttribute` is the base gate:

- `[Requires(ModContentPack.OdysseyModPackageId)]`

Shorthand attributes exist for common DLC gates:

- `[RequiresRoyalty]`
- `[RequiresIdeology]`
- `[RequiresBiotech]`
- `[RequiresAnomaly]`
- `[RequiresOdyssey]`

Use these on recorder tests that depends a specific DLC to function. The test will be skipped with a warning if DLC
is not loaded.

## Parameterization

- `[DebugValues(...)]`: run the same test body with multiple integer inputs.

Use this when a recorder should behave the same across a small range of counts or values.

## Discovery and Filtering

- `[SkipTest]`: opt a test out of normal execution.
- `[TestTag("...")]`: annotate tests for execution in group.

