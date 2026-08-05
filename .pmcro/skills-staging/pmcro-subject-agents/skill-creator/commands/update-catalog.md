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
`../references/marketplace-schema-notes.md`; it's a separate convention
for the .NET runtime's materializer, not a twin of this file.

**This command performs the TYPE1 action itself** -- per
`../../orchestrator/references/hil-gating.md`, writing a live entry into
`marketplace.json` is exactly what makes something TYPE1. That means this
command only runs *after* human approval of the package, never before. A
package sitting on disk but not yet run through this command is the
correct `needs-approval` staging state -- files exist, nothing points at
them yet. Do not invoke this command speculatively "to see if it fits" --
that speculative check belongs entirely to `validate-skill.md`, which is
TYPE2 and safe to run anytime.
