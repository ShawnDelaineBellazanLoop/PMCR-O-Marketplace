#!/usr/bin/env python3
"""
git_local.py

Whitelisted wrapper around local-only git operations for the `git` pmcro-skill.
Deliberately excludes anything that touches a remote (push/pull/fetch/clone/remote)
or rewrites history (rebase -i, reset --hard, push --force, amend). See
references/safety-notes.md for why.

Usage:
    python3 git_local.py <repo-path> status
    python3 git_local.py <repo-path> init
    python3 git_local.py <repo-path> add <path-or-'all'>
    python3 git_local.py <repo-path> commit "<message>"
    python3 git_local.py <repo-path> branch [--list | --create <name> | --switch <name>]
    python3 git_local.py <repo-path> diff [--staged]
    python3 git_local.py <repo-path> log [-n <count>]
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

WHITELIST = {"status", "init", "add", "commit", "branch", "diff", "log"}


def run_git(repo_path: Path, args: list[str]) -> subprocess.CompletedProcess:
    return subprocess.run(
        ["git", "-C", str(repo_path), *args],
        capture_output=True,
        text=True,
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Whitelisted local git wrapper.")
    parser.add_argument("repo_path", type=str)
    parser.add_argument("subcommand", choices=sorted(WHITELIST))
    parser.add_argument("rest", nargs=argparse.REMAINDER)
    ns = parser.parse_args()

    repo_path = Path(ns.repo_path)
    sub = ns.subcommand
    rest = ns.rest

    if sub not in WHITELIST:
        print(f"REJECTED — '{sub}' is not a whitelisted subcommand.", file=sys.stderr)
        return 2

    if sub == "init":
        repo_path.mkdir(parents=True, exist_ok=True)
        result = run_git(repo_path, ["init"])
    elif sub == "status":
        result = run_git(repo_path, ["status", "--short", "--branch"])
    elif sub == "add":
        target = rest[0] if rest else "."
        paths = ["."] if target == "all" else [target]
        result = run_git(repo_path, ["add", *paths])
    elif sub == "commit":
        if not rest:
            print("REJECTED — commit requires a message.", file=sys.stderr)
            return 2
        message = rest[0]
        result = run_git(repo_path, ["commit", "-m", message])
    elif sub == "branch":
        if not rest or rest[0] == "--list":
            result = run_git(repo_path, ["branch", "--list"])
        elif rest[0] == "--create" and len(rest) > 1:
            result = run_git(repo_path, ["checkout", "-b", rest[1]])
        elif rest[0] == "--switch" and len(rest) > 1:
            result = run_git(repo_path, ["checkout", rest[1]])
        else:
            print("REJECTED — unrecognized branch arguments.", file=sys.stderr)
            return 2
    elif sub == "diff":
        args = ["diff", "--cached"] if "--staged" in rest else ["diff"]
        result = run_git(repo_path, args)
    elif sub == "log":
        count = "10"
        if "-n" in rest:
            idx = rest.index("-n")
            if idx + 1 < len(rest):
                count = rest[idx + 1]
        result = run_git(repo_path, ["log", f"-n{count}", "--oneline"])
    else:
        print(f"REJECTED — '{sub}' has no handler.", file=sys.stderr)
        return 2

    if result.stdout:
        print(result.stdout, end="")
    if result.stderr:
        print(result.stderr, end="", file=sys.stderr)

    return result.returncode


if __name__ == "__main__":
    sys.exit(main())
