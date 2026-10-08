# PMCR-O QA (built incrementally from nothing)

Status: draft, uncommitted. Built in passes. Each item states its evidence and how to re-run it.
Sources: the 76-item backlog (`PMCR-O ideas, refined`), the founder's laws from the design transcript, and the repo as checked out.

Vocabulary (agreed):
- **replay**: check each recorded frame against the current environment. Records MATCH or MISMATCH. Changes nothing.
- **execute**: the autonomous mode. Runs inside a standing approval (#58). Asks only at the edges: spend, external send, install, delete.
- **simulation**: a run that is labelled as one. It is never evidence (#25).

## Pass 1: inventory (what exists in this checkout)

| Item | Found | Evidence |
|---|---|---|
| Trail frames | yes, 310 JSON files | `find .pmcro/trails -name '*.json*' \| wc -l` |
| ADR 0013 (simulated trails are not evidence) | **no** | no `docs/` folder in this checkout |
| Eval quality gate (`eng/eval-quality/check_eval_quality.py`) | **no** | AGENTS.md names it; the path is absent |
| `.agents/skills` in this checkout | **no** | path absent on this drive |
| CONTRIBUTING.md, AGENTS.md | yes | repo root |

## Pass 2: laws checked against the 310 frames

| Law | Test | Result | Status |
|---|---|---|---|
| No absolute paths in trails | `grep -rlE '[A-Za-z]:\\\\\|[A-Za-z]:/' .pmcro/trails` | **111 files** | FAIL |
| Trail voice: no first-person "I" in text | `grep -rlE '"[^"]*\bI (will\|am\|did\|think)\b'` | **16 files** | FAIL |
| Speaker, addressee, act recorded (#69) | `grep -rl '"addressee"\|"speaker"'` | **0 files** | NOT BUILT |
| Replay result recorded (MATCH or MISMATCH) | `grep -rl 'MATCH'` | **0 files** | NOT BUILT |
| Trail append-only (founder law) | frames are never rewritten | not measured here | UNVERIFIED |
| Checker independent of maker | a checker record names a different agent than the maker | not measured here | UNVERIFIED |

## Pass 3: gaps, by priority

1. **Absolute paths (111 files).** Highest priority, because it breaks the law outright and blocks any export (#68). Frames are append-only, so do not rewrite them. Add a correction line that supersedes the path, and make the replay tool read the corrected view.
2. **No replay result on any frame.** Replay can't be scored until frames record MATCH or MISMATCH.
3. **No speaker, addressee, act (#69).** Needed before a frame can be read by another agent.
4. **"I" in trail text (16 files).** Lower risk, but the law is explicit. Correct by superseding line, as in item 1.
5. **Missing ADR 0013 and the eval gate.** Documents and gates the repo refers to but does not contain. Either restore them or remove the references.
6. **Checker independence and append-only are unverified.** Needs a check, not an assumption.

## Pass 4: plan (ordered; nothing is edited yet)

1. Write the frame schema (workstream 1): fields `speaker`, `addressee`, `act`, `replay_result`, `paths` (relative only), `score`.
2. Write a checker for the five laws above. It runs on every frame and reports, without editing.
3. Add correction lines for the 111 absolute-path files and the 16 first-person files. Do not rewrite originals.
4. Build replay to write MATCH or MISMATCH on each frame.
5. Restore or remove ADR 0013 and the eval gate. Decide which.

## Open decisions for the founder

- Canonical repo (#6). This QA is written against the checkout here.
- Whether the 16 "I" frames are corrected or kept as historical and flagged.
- Standing-approval policy (#58) before any execute mode runs.

## Re-run

```
cd /home/claude/pmcr-o-marketplace
find .pmcro/trails -name '*.json*' | wc -l
grep -rlE '[A-Za-z]:\\\\|[A-Za-z]:/' .pmcro/trails | wc -l
grep -rlE '"[^"]*\bI (will|am|did|think)\b' .pmcro/trails | wc -l
grep -rl '"addressee"\|"speaker"' .pmcro/trails | wc -l
grep -rl 'MATCH' .pmcro/trails | wc -l
```
