# Git — Whitelisted Command Reference

`scripts/git_local.py` only ever runs the seven subcommands below. Nothing else is
reachable through this skill — see `safety-notes.md` for why the boundary is drawn here.

## `status`

```
python3 git_local.py <repo-path> status
```
Runs `git status --short --branch`. Always safe to run first; read-only.

## `init`

```
python3 git_local.py <repo-path> init
```
Creates `<repo-path>` if it doesn't exist, then runs `git init`. Does not stage or
commit anything — that's a separate, explicit step.

## `add`

```
python3 git_local.py <repo-path> add <path>
python3 git_local.py <repo-path> add all
```
Stages a specific path, or everything (`all` → `git add .`).

## `commit`

```
python3 git_local.py <repo-path> commit "<message>"
```
Requires a message; the script rejects the call if one isn't provided. Does not
support `--amend` — amending an existing commit is out of scope (see safety notes).

## `branch`

```
python3 git_local.py <repo-path> branch --list
python3 git_local.py <repo-path> branch --create <name>
python3 git_local.py <repo-path> branch --switch <name>
```
`--list` is read-only. `--create` runs `git checkout -b <name>` (create + switch in one
step). `--switch` runs `git checkout <name>` on an existing branch.

## `diff`

```
python3 git_local.py <repo-path> diff
python3 git_local.py <repo-path> diff --staged
```
Unstaged diff by default; `--staged` shows `git diff --cached` instead.

## `log`

```
python3 git_local.py <repo-path> log
python3 git_local.py <repo-path> log -n 20
```
Defaults to the last 10 commits, one line each. `-n <count>` changes how many.

## Everything Else Is Rejected

Any subcommand not in this list (`push`, `pull`, `fetch`, `clone`, `remote`, `rebase`,
`reset`, `merge`, `cherry-pick`, `tag`, `stash`, etc.) causes `git_local.py` to exit
with code `2` and print `REJECTED`. That is intentional — see `safety-notes.md`.
