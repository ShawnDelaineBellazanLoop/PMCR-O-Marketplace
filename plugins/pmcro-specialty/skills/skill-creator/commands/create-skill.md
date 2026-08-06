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
