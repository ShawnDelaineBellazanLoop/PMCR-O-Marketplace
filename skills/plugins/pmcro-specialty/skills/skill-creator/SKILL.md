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


## Workflow

This section contains the executable workflows formerly in commands/.


### create-skill
Package a new command, sub-agent, domain, or PMCR-O role-skill into this repo's catalog convention. Usage: /skill-creator:create-skill <shape: command|agent|domain|role-skill> <target> <spec>

---
description: "Package a new command, sub-agent, domain, or PMCR-O role-skill into this repo's catalog convention. Usage: /skill-creator:create-skill <shape: command|agent|domain|role-skill> <target> <spec>"
---
```
shape: <first argument>
target_package: <second argument -- existing package for command/agent, new name for domain/role-skill>
spec: <remaining arguments>
repo_path: <the target repo root>
```

Invoke the `skill-creator` skill (`../SKILL.md`) -- it resolves
dependencies via `dependency-resolver` first, then packages per the shape
given. Produces files on disk only; does **not** update
`.claude-plugin/marketplace.json` (that's `update-catalog.md`,
deliberately separate so a package can be drafted and reviewed before
it's registered anywhere).


### update-catalog
Register a validated package into .claude-plugin/marketplace.json. Usage: /skill-creator:update-catalog <package-name>

---
description: "Register a validated package into .claude-plugin/marketplace.json. Usage: /skill-creator:update-catalog <package-name>"
---
```
package_name: <first argument>
repo_path: <the target repo root>
```

Run `validate-skill.md` first. Do not proceed if it failed.

Then add the plugin entry to `.claude-plugin/marketplace.json`. Do not
touch `.agents/plugins/marketplace.json` -- see
`references/marketplace-schema-notes.md`; it's a separate convention
for the .NET runtime's materializer, not a twin of this file.

**This command performs the TYPE1 action itself** -- per
`../../../pmcro-engine/skills/orchestrator/references/hil-gating.md`, writing a live entry into
`marketplace.json` is exactly what makes something TYPE1. That means this
command only runs *after* human approval of the package, never before. A
package sitting on disk but not yet run through this command is the
correct `needs-approval` staging state -- files exist, nothing points at
them yet. Do not invoke this command speculatively "to see if it fits" --
that speculative check belongs entirely to `validate-skill.md`, which is
TYPE2 and safe to run anytime.


### validate-skill
Validate a packaged skill against Claude Code's own plugin schema before it's registered. Usage: /skill-creator:validate-skill <path-to-package>

---
description: "Validate a packaged skill against Claude Code's own plugin schema before it's registered. Usage: /skill-creator:validate-skill <path-to-package>"
---
```
package_path: <first argument, relative to repo_path>
```

Run `claude plugin validate .` at `repo_path` -- Claude Code's own
marketplace/plugin schema (naming, `source` paths, category values).

If this repo also maintains a `catalog/skills.schema.json`, validate the
package's prospective entry against that too and report both results
explicitly, even if one clearly failed and checking the other feels
redundant -- a package that's schema-valid in one system but not the
other is exactly the kind of gap a two-check rule exists to catch.


