# Agent Skills Convention (agentskills.io)

This repo's skills follow the open Agent Skills standard
(`agentskills.io`), the same convention used elsewhere across Shawn's
projects.

## Anatomy

```
skill-name/
├── SKILL.md (required)
│   ├── YAML frontmatter -- name + description required, minimum
│   └── Markdown instructions
└── Bundled resources (optional)
    ├── scripts/    -- executable code for deterministic/repetitive tasks
    ├── references/ -- docs loaded into context only as needed
    └── assets/     -- templates, schemas, icons used in output
```

## The `metadata` Field -- Where `provides`/`requires` Actually Live

Important: the spec does **not** define a first-class `provides`/
`requires` field. It defines one optional `metadata` field -- a generic
string-to-string map for properties the spec itself doesn't standardize.
This repo's dependency contract lives there:

```yaml
---
name: maker
description: "..."
metadata:
  pmcro_provides: "maker"
  pmcro_requires: ""
---
```

Do not invent a new top-level frontmatter key for this (e.g. a bare
`provides:` at the frontmatter root) -- it isn't part of the spec and
tooling that only understands standard fields won't see it. Nest it under
`metadata` as shown.

## Progressive Disclosure

Three loading levels, same principle whether the package is a domain
scope or a PMCR-O role-skill:

1. **Metadata** (name + description) -- always in context, ~100 words
2. **SKILL.md body** -- loaded when the skill triggers, <500 lines ideal
3. **Bundled resources** -- loaded only as needed, referenced explicitly
   from the SKILL.md body with guidance on when to read them

If a SKILL.md is approaching 500 lines, split into a `references/` file
and point to it rather than letting the body grow further -- see
`skills/orchestrator/SKILL.md` for an example of a package that does this
(trail schema, EC registry, ralph-loop mechanics, and HIL gating are all
`references/`, not inlined into the SKILL.md body).
