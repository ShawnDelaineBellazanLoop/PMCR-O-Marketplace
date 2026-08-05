---
name: skill-creator
description: "USE FOR: packaging a new command, sub-agent, domain, or PMCR-O role-skill into this repo's catalog convention -- creating the SKILL.md frontmatter, commands/references/scripts layout, and provides/requires metadata, then keeping catalog/skills.json and marketplace.json in sync. This is the packaging step every /ceo:evolve-colony cycle delegates to; it never runs standalone without CTO architecture review and CEO approval already in the loop. DO NOT USE FOR: deciding whether a capability gap should become a command vs. a sub-agent vs. a new domain (that's ceo's agent-router during evolve-colony's routing step) or publishing the result (that's a separate HIL-gated approval step this skill never performs itself)."
metadata:
  pmcro_provides: "skill-creator"
  pmcro_requires: "dependency-resolver"
compatibility: "Read/write access to catalog/ and marketplace.json at repo_path. No other runtime dependency."
---

# Skill Creator (Colony Packaging)

Packages new Colony capability into this repo's existing convention. Does
not decide *whether* something should be built (that's upstream, in
`/ceo:evolve-colony`'s routing step) and does not *publish* what it
packages (that's downstream, gated by human approval). This skill's entire
job is the middle: turn an approved shape into a correctly-structured
package on disk.

## Invocation Contract

```
shape: command | agent | domain | role-skill
target_package: <existing package this attaches to, or new package name>
spec: <what the new command/agent/domain/role-skill should do>
repo_path: <the target repo root>
```

## Before Writing Anything

Invoke `dependency-resolver` with `needs: <target_package>` to confirm the
target package actually exists (for `command`/`agent` shapes) or that the
proposed new package name doesn't collide with an existing
`pmcro_provides` value (for `domain`/`role-skill` shapes).

## Packaging By Shape

### `command`
Add `skills/<target_package>/commands/<name>.md` matching the exact
frontmatter shape already in use across this repo: a `description` field
with a `Usage:` line. See any existing file under `skills/*/commands/`
for the pattern to match verbatim, not approximately.

### `agent`
Add `skills/<target_package>/agents/<name>.md` with `name`, `description`,
`tools`, and `model` frontmatter, matching an existing agent file's shape.

### `domain`
Full new package under `skills/<name>/`: `SKILL.md` (with explicit
Owns/Does Not Own scope), `commands/`, `agents/`.

### `role-skill`
Full new package under `skills/<name>/`, same layout as
`planner`/`maker`/`checker`/`reflector` -- passive, stateless,
`metadata.pmcro_provides`/`pmcro_requires` declared, no sequencing logic
of its own. This is the shape `/ceo:evolve-colony` would use if a gap
turns out to need a genuinely new PMCR-O role rather than fitting one of
the existing four.

## After Packaging: Sync the Catalog (never partially)

`catalog/skills.json` (if this repo maintains one) and
`.claude-plugin/marketplace.json` change together, in the same pass, or
not at all. See `references/marketplace-schema-notes.md` for the current
marketplace.json fields (`version`, `renames`, `hooks`, `mcpServers`,
`lspServers`) worth setting on new entries.

## Validate Before Handing Back

Run `claude plugin validate .` (checks Claude Code's own marketplace
schema) **and** confirm any repo-local `catalog/skills.json` entry against
its own schema if one exists -- a different surface, both must pass, one
passing doesn't imply the other does.

## Hard Stop

This skill hands its output back as `needs-approval`, never writes it as
`accept`. Whatever invoked this skill is responsible for sealing the
trail that way and routing to a human approval step -- packaging
correctly is not the same as being approved to publish.
