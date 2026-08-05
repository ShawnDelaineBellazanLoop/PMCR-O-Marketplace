# Trail Skills Verification — 2026-07-23

## Verified against source
- `LoopFrame.cs` — read full file, confirmed all frame types and field names
- `ITrailWriter.cs` — confirmed on-disk contract
- `FileTrailWriter.cs` — confirmed SealAsync behavior
- Live trail `.pmcro/trails/cto/4782c166-.../` — confirmed matches real schema exactly

## Created
- `skills/trail-indexer/SKILL.md` — real frame vocabulary, correct field mappings, Type-2 read-only rules
- `skills/pattern-learner/SKILL.md` — real frame vocabulary, evidence sources corrected, three-occurrence rule, disposition enum corrected to Accept/Retry/Halt

## Key corrections from Tier-0 template to real schema
1. `SeedIntentFrame{subject,goal}` → `00-frame.json{seed_intent}` (single field)
2. `DispositionFrame{accept,reject,superseded}` → `disposition.json{Disposition} ∈ {Accept,Retry,Halt}`
3. `ObservationFrame.evidence` → does not exist; use `MakerFrame.step_results[].ground_truth.evidence`
4. `CheckerFrame.checkItems[].evidence` → `CheckItem.failure_evidence` (fail only); passing items read from MakerFrame

## Status: COMPLETE
Both skills are now schema-correct. Ready for MAF agent loading via AgentSkillsProvider.