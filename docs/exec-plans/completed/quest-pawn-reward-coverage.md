# Quest Pawn Reward Coverage

Implemented explicit `PH_QuestPawnArrived` reward descriptions for every quest root in scope that can produce a generated `Reward_Pawn` through the normal reward-choice pipeline.

## Notes

- Added reward-specific rulepack entries for `Mission_BanditCamp`, `BuildMonument_Basic`, `BuildMonument_TimeProtect`, `Hospitality_Animals`, `Hospitality_Joiners`, `Hospitality_Prisoners`, `ThreatReward_MechPods_MiscReward`, `ThreatReward_Raid_MiscReward`, `ShuttleCrash_Rescue`, and `SanguophageMeetingHost`.
- Kept the existing explicit reward coverage for `OpportunitySite_BanditCamp`, `TradeRequest`, and `PawnLend`.
- Reused the bandit-camp world-object grammar for `Mission_BanditCamp` by broadening `QuestPawnArrivedComp_BanditCamp` to match both quest roots.
- Left implicit or non-standard pawn-join flows out of the implementation pass, including wanderer, refugee pod crash, joiner-threat, intro-joiner, downed-refugee, prisoner-willing-to-join, hospitality-refugee, and the `SanguophageMeetingHost` join-offer branch.
- Did not add new tests beyond the existing reward-focused coverage because the remaining reward quests are not simple setup-and-validate cases with the current in-game test DSL.
