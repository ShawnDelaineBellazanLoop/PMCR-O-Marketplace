# HIL Gate Record — EC-005 Confirmation

**Date:** 2026-07-22  
**Approved by:** Shawn (Architect of Cognition, Tooensure LLC)  
**Trail reference:** td-011-seed-to-true-intent, cycle 1  
**Gate type:** TYPE1 — governance spec change

## What was confirmed

**1. Two-class EarnedConstraint distinction — ADOPTED as Colony Law**

- **Type A** — Failure pattern. Requires recurrence threshold. Documented in constraint-ledger.md. Nullable on ReflectorFrame when no failure pattern was caught this cycle.
- **Type B** — Refinement. Produced by every cycle. Lives in Baton.NextSeedIntent. The better-bounded shape of what was meant. NOT nullable on Accept or Retry.

**2. Governance rule — ADOPTED as Colony Law**

> A sealed trail with `Disposition: Accept` or `Disposition: Retry` and `Baton: null` is a governance violation. The Reflector did not reflect — it terminated. The Baton is mandatory on Accept and Retry. Only `Disposition: Halt` may carry a null Baton (the loop has intentionally stopped).

## Required follow-on TYPE1 changes (approved for implementation)

1. Update `Frames.cs` — add XML doc comment to `Baton?` field: "Required on Accept and Retry. Null only on Halt. A null Baton on Accept is a governance violation per EC-005."
2. Update `reflector.md` agent definition — add EC-005 to standing constraints section.
3. Update `constraint-ledger.md` — append EC-005 in five-part format.
4. Update `TrailStore.AppendCycleAsync` — add runtime validation: throw if cycle.Reflector.Disposition != Halt && cycle.Reflector.Baton == null.

## Shawn's confirmation

Stated: "yes" — 2026-07-22, this session.
