---
name: career-evidence
description: "USE FOR: gathering verifiable evidence of Shawn's real programming
  history (git commit dates in local repos, public GitHub account history,
  corpus documents) and turning it into an honest, resume-ready timeline and
  project summary. Never fabricates dates, employers, or job titles -- only
  reports what evidence actually supports, and flags gaps where evidence is
  missing or inconclusive. DO NOT USE FOR: writing persuasive/embellished
  resume copy not backed by evidence -- that's a separate, human-reviewed
  step after this skill's output."
metadata:
  pmcro_provides: "career-evidence"
  pmcro_requires: "none"
compatibility: "Read access to local repos under W:\\PMCR-O and public GitHub
  data. No write access to any external system (job applications, profiles)."
---

# Career Evidence

Builds an evidence-backed career timeline. Every claim in the output must
trace to a specific source (a commit date, a GitHub join date, a document
in AI-Knowledge-Corpus). No source, no claim.

## Inputs

- `github_usernames`: list of GitHub handles to check (old + current)
- `local_repo_paths`: local git repos to pull commit history from
  (default: everything under `W:\PMCR-O`)
- `target_role`: the job/company this evidence will support (e.g. "TextNow
  backend engineer") -- used only to decide what's worth surfacing, never
  to shade the dates

## Steps

1. **Local git history**: for each repo, pull first-commit and last-commit
   dates, commit count, and languages touched. Report the actual date
   range found -- do not extrapolate beyond it.
2. **GitHub account history**: for each username, fetch public join date,
   contribution graph, and repo list via GitHub's public API/web pages.
3. **Corpus cross-reference**: scan `AI-Knowledge-Corpus/` for dated
   artifacts (conversations, decisions, reports) that corroborate or
   extend the timeline beyond what git history alone shows.
4. **Reconcile**: where sources conflict or a gap exists (e.g. local repo
   history is short but corpus suggests older origins), state the
   discrepancy explicitly rather than picking the more favorable number.
5. **Output**: one `MakerFrame` per evidence claim, matching this repo's
   real `Frame` schema (`ProjectName.Agents.AI.Core/Frames.cs`) rather than
   informal prose:

   ```
   MakerFrame {
     ActionSummary: "<the claim, e.g. 'active on this repo since Jul 2026'>"
     GroundTruth {
       VerificationMethod: "<e.g. 'git log --reverse'>"
       Verified: true|false
       Evidence: "<the actual output -- date, commit hash, GitHub join date>"
     }
   }
   ```

   Then one `CheckerFrame` stating whether the full claim set actually
   supports the target timeline (`CriteriaMet`, `Rationale`,
   `CriteriaEvaluated`), and one `ReflectorFrame` with `Disposition` --
   `Halt` with a `HaltReason` if evidence is too thin to proceed, not a
   flattering guess. A claim with `Verified: false` still gets reported,
   never dropped, and never silently upgraded to `true`.

## Colony Laws

- Never write a date, employer, or title into resume-facing output
  without a traceable source line.
- A conflict between sources is surfaced, never silently resolved in
  whichever direction is more flattering.
- This skill produces evidence, not final resume copy -- Shawn reviews
  and approves before anything goes to an employer.


## Workflow

This section contains the executable workflows formerly in commands/.


### timeline
Build an evidence-backed career timeline from git, GitHub, and corpus. Usage: /career-evidence:timeline <target-role>

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


