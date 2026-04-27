# Asset and Reference Notes

Actionable references for future visual or content work.

## Icon Sources

- `Assets\Resources\textures\things\mote\thoughtsymbol`
- `Assets\Resources\textures\things\mote\speechsymbols`
- `Assets\Resources\textures\things\mote\battlesymbols`
- `Assets\Resources\textures\ui`
- `2636329500`
- `3268401022`
- `rimworld\assets-royalty\Assets\data\royalty\textures`
- `rimworld\assets-biotech\Assets\data\biotech\textures`
- `rimworld\assets-ideology\Assets\data\ideology\textures`
- `rimworld\assets-anomaly\Assets\data\anomaly\textures`
- `rimworld\assets-odyssey\Assets\data\odyssey\textures`

## Reference Trails

Use these as the starting point to track down potential event for a new history record

- `Letter.xml`
- `Core\Languages\English\Keyed\Messages.xml`
- searches over `<IncidentDef>`
- `QuestScriptDefs/**.xml`
- Tale-based signals worth mapping from `TaleRecorder.RecordTale()`.
- PrisonerInteractionModeDefOf -> tale events?
- Review `HistoryEventDefOf.cs` and `.RecordEvent(` call sites for upstream signals worth converting into typed events.
- Review `JobDefOf.cs` and similar hidden entrypoint for story-worthy behaviors such as OfferHelp.
- `.PassToWorld(`