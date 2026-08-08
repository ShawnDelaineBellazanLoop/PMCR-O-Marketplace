---
name: git
description: "USE FOR: local version-control operations on a repository already present on disk -- init, status, add/stage, commit, branch create/switch/list, diff, log. DO NOT USE FOR: any remote operation (push, pull, fetch, clone, remote add) or GitHub-specific actions (PRs, issues, releases) -- explicitly out of scope for this skill; those are deliberate, human-confirmed actions run directly, never automated through this whitelist. DO NOT USE FOR: resolving merge conflicts automatically or force-rewriting history (rebase -i, reset --hard, push --force) without explicit human confirmation per commit. INVOKES: run scripts/git_local.py against the target repo path with a whitelisted subcommand, report the resulting state (staged files, commit hash, branch, diff summary) back to the requester."
compatibility: "Requires git installed and on PATH. Python 3.12+ to run scripts/git_local.py. Operates only on repositories already initialized or explicitly requested to be initialized on the local filesystem."
---

# Git

## Trigger On

- "commit this" / "stage these changes" / "what's the git status"
- "create a branch for X" / "switch to branch X" / "list branches"
- "show me the diff" / "show recent commits" / "git log"
- "init a git repo here"

## Value

- Gives an agent a safe, whitelisted way to perform everyday local git operations
  without needing raw shell access or risking an accidental remote action.
- Every operation goes through `scripts/git_local.py`, so the set of possible commands
  is fixed and auditable rather than "whatever git subcommand the agent decides to run."
- Keeps local version control fast to invoke for routine work (status, diff, commit)
  instead of round-tripping through a general-purpose terminal-agent each time.
- Mirrors the same convention already established in the sibling `pmcro-skills`
  catalog (`Tools/Agent-Foundry/skills/git`) -- same whitelist, same safety posture,
  ported here so the Marketplace catalog has parity.

## Do Not Use For

- Anything that touches a remote: `push`, `pull`, `fetch`, `clone`, `remote add/remove`.
  This skill is local-only by design -- see `references/safety-notes.md`.
- GitHub-specific actions (opening PRs, issues, releases, reviews). That's a separate
  concern from local git and, if needed later, belongs in its own skill.
- History rewriting (`rebase -i`, `reset --hard`, `push --force`, `commit --amend` on
  already-shared commits) without explicit per-action human confirmation.
- Merge conflict resolution -- this skill can report that a conflict exists; it does not
  auto-resolve conflicting hunks.

## Inputs

- Target repository path (must already exist on disk, or be the path to initialize).
- The operation requested (status / add / commit / branch / diff / log / init).
- For `commit`: a commit message. For `branch`: the branch name and whether to create,
  switch, or list. For `add`: which files/paths to stage (or "all").

## Quick Start

1. Confirm the target path exists and is (or should become) a git repository.
2. Run `python3 scripts/git_local.py <repo-path> status` first to see current state
   before mutating anything.
3. Run the specific whitelisted subcommand needed (`add`, `commit`, `branch`, `diff`,
   `log`, `init`).
4. Report the resulting state back -- new commit hash, current branch, staged files --
   not just "done."

## Workflow

1. **Observe** -- determine what local git state the requester needs to see or change.
2. **Plan** -- pick the single whitelisted subcommand that accomplishes it; if the
   request implies a remote or history-rewriting action, stop and say so instead of
   improvising a workaround.
3. **Create** -- invoke `scripts/git_local.py <repo-path> <subcommand> [args]`.
4. **Check** -- parse the script's output; confirm the operation actually succeeded
   (e.g. a commit produced a hash, a branch switch changed `HEAD`).
5. **Reflect** -- if the script rejects the subcommand (not on the whitelist) or git
   itself errors (e.g. nothing staged to commit), report the exact reason rather than
   retrying blindly.
6. **Report** -- return the concrete result (hash / branch / diff / log lines) to the
   requester.

## Bootstrap When Missing

If the target path is not yet a git repository and the requester wants one:

1. Confirm the path exists on disk as a plain directory first.
2. Run `python3 scripts/git_local.py <path> init`.
3. Do not auto-run `add`/`commit` immediately after `init` -- confirm with the requester
   what should go into the first commit.

## Deliver

- The concrete git state change requested (a commit, a new/switched branch, a diff or
  log printed back) plus the exact command that produced it.
- For `commit`: the resulting commit hash and message.
- For `branch`: the current branch after the operation.

## Validate

- The subcommand used is on the whitelist in `scripts/git_local.py`
  (`init`, `status`, `add`, `commit`, `branch`, `diff`, `log`) -- nothing else runs.
- No remote-touching git invocation occurred anywhere in the operation.
- `commit` was only run after confirming what's staged (via `status` or `diff`), not
  blind.

## Ralph Loop

Plan the single subcommand -> execute via `git_local.py` -> review its stdout/stderr for
actual success (not just exit code 0 -- e.g. "nothing to commit" is exit 1 but not a
crash) -> if it failed for a fixable reason (wrong path, nothing staged), fix and retry
once; if it failed because the request was out of scope (remote op), stop and report
instead of finding a workaround.

## Required Result Format

- `status`: `done` | `blocked` | `out_of_scope`
- `operation`: the subcommand run
- `result`: commit hash / branch name / diff or log output, as applicable
- `remaining`: anything the requester still needs to decide (e.g. commit message
  wording), or `none`

## Load References

- [references/git-command-reference.md](references/git-command-reference.md) -- the
  exact whitelisted commands and their flags
- [references/safety-notes.md](references/safety-notes.md) -- why remote and
  history-rewriting operations are excluded, and what to do if one is requested

## Example Requests

- "What's the git status in W:\PMCR_O\PMCR-O-Marketplace?"
- "Stage all changes and commit them with message 'add git skill'."
- "Create a branch called feature/git-skill and switch to it."
- "Show me the last 5 commits."
- "Show me the diff of unstaged changes."
