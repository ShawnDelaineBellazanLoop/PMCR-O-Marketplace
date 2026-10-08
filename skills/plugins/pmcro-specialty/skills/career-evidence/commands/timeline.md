---
description: "Build an evidence-backed career timeline from git, GitHub, and corpus. Usage: /career-evidence:timeline <target-role>"
---
# /career-evidence:timeline

```
target_role: <first argument – e.g. "TextNow backend engineer">
github_usernames: <optional list of handles>
local_repo_paths: <optional; default everything under W:\PMCR-O>
repo_path: <the target repo root>
```

Produce an honest, resume-ready timeline and project summary grounded only in verifiable evidence.

## Steps

1. For each local repo: first-commit and last-commit dates, commit count, languages touched. Report the actual date range found — do not extrapolate.
2. For each GitHub username: public join date, contribution graph, repo list via public API / web pages.
3. Scan AI-Knowledge-Corpus for dated artifacts that corroborate or extend the timeline.
4. Where sources conflict or a gap exists, state the discrepancy explicitly rather than picking the more favorable number.
5. Output one MakerFrame per evidence claim (ActionSummary + GroundTruth with VerificationMethod / Verified / Evidence), then one CheckerFrame (CriteriaMet / Rationale / CriteriaEvaluated), then one ReflectorFrame with Disposition (Halt with HaltReason if evidence is too thin).

## Guardrails
- Never write a date, employer, or title into resume-facing output without a traceable source line.
- A conflict between sources is surfaced, never silently resolved in the flattering direction.
- This skill produces evidence, not final resume copy — human review is required before anything goes to an employer.
- A claim with Verified: false is still reported; it is never dropped and never silently upgraded to true.
