---
name: plugin-creator
description: "USE FOR: scaffolding a brand-new top-level plugin package under plugins/<name>/ -- .claude-plugin/plugin.json, .codex-plugin/plugin.json, commands/, skills/ -- when a capability doesn't fit inside any existing plugin. DO NOT USE FOR: adding a command/agent/domain/role-skill inside an existing plugin (that's skill-creator's job) or registering the finished plugin into .claude-plugin/marketplace.json (that's skill-creator:update-catalog, run after this skill hands back needs-approval)."
metadata:
  pmcro_provides: "plugin-creator"
  pmcro_requires: "dependency-resolver, skill-creator"
compatibility: "Read/write access to plugins/ and .claude-plugin/marketplace.json at repo_path. No other runtime dependency."
---

# Plugin Creator (New Top-Level Package)

Scaffolds an entirely new `plugins/<name>/` package from nothing. This is
a level up from `skill-creator`: skill-creator packages a command, agent,
domain, or role-skill *inside* an existing plugin; plugin-creator exists
because that's not the right tool when nothing to attach to exists yet.

## Invocation Contract

```
plugin_name: <new top-level plugin folder name, kebab-case>
spec: <what this plugin is for, and its first skill(s)>
repo_path: <the target repo root, e.g. this marketplace repo>
```

## Before Writing Anything

1. Invoke `dependency-resolver` with `needs: <plugin_name>` to confirm the
   name doesn't collide with an existing entry in
   `.claude-plugin/marketplace.json` or an existing `plugins/` directory.
2. Diff against a known-good reference plugin in this same repo rather
   than re-deriving structure from upstream docs -- this repo already has
   two proven shapes (`plugins/dotnet` for a lean single-skill plugin,
   `plugins/pmcro-csuite` for a multi-skill domain plugin). Pick whichever
   is the closer match to `spec`.

## Scaffold, In Order

1. `plugins/<name>/.claude-plugin/plugin.json`
   ```json
   {
     "name": "<name>",
     "version": "1.0.0",
     "description": "<one line>",
     "skills": [
       { "name": "<skill-name>", "path": "skills/<skill-name>/SKILL.md" }
     ]
   }
   ```
   Write this file UTF-8 **without a BOM**. A leading BOM makes the file
   invalid to strict JSON parsers and is exactly what breaks Claude Code's
   "Add marketplace" / plugin load with no useful error -- confirmed twice
   in this repo's trail history (marketplace.json and
   pmcro-csuite/plugin.json both shipped with a BOM). Verify with a
   byte-level read of the first 3 bytes before handing back; `EF BB BF`
   means fix it before proceeding.

2. `plugins/<name>/.codex-plugin/plugin.json` -- only if this repo's other
   plugins carry one (check first; this repo's convention pairs
   `.claude-plugin` with `.codex-plugin` for cross-tool loadability, per
   trail `2026-08-05-marketplace-plugin-completion`). Minimal shape:
   ```json
   {
     "name": "<name>",
     "version": "1.0.0",
     "description": "<one line>",
     "skills": ["./skills/"]
   }
   ```

3. `plugins/<name>/skills/<skill-name>/SKILL.md` -- hand off to
   `skill-creator` with `shape: domain` or `shape: role-skill` (whichever
   `spec` calls for) rather than duplicating that logic here. This skill
   owns the plugin shell; skill-creator owns everything inside `skills/`.

4. `plugins/<name>/commands/` -- top-level, not nested under
   `skills/<name>/commands/`. Only create if `spec` calls for slash
   commands; an empty `commands/` dir is not required (confirmed: several
   proven plugins in this repo, e.g. `pmcro-engine`, ship without one).

## Validate Before Handing Back

Run `claude plugin validate .` at `repo_path`. Also re-open both
`plugin.json` files and confirm no BOM, `source`/`path` fields match the
schema actually in use (not a remembered-from-docs schema -- check a
sibling plugin), and JSON parses clean.

## Hard Stop

This skill hands its output back as `needs-approval`, same as
skill-creator. It never calls `skill-creator:update-catalog` itself and
never edits `.claude-plugin/marketplace.json` directly -- registering a
new plugin into the live marketplace is the TYPE1 step, gated by human
approval, and stays a separate command run after this one.

## Workflow

### create-plugin
Scaffold a new top-level plugin package. Usage: /plugin-creator:create-plugin <name> <spec>

---
description: "Scaffold a new top-level plugin package under plugins/<name>/. Usage: /plugin-creator:create-plugin <name> <spec>"
---
```
plugin_name: <first argument>
spec: <remaining arguments>
repo_path: <the target repo root>
```

Invoke the `plugin-creator` skill (`../SKILL.md`). Produces files on disk
only; does not touch `.claude-plugin/marketplace.json`. Follow with
`skill-creator:validate-skill` and, after human approval,
`skill-creator:update-catalog` to register it.
