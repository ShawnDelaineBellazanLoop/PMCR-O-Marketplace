# Self-Referential Upgrade Checkpoint

## Batch 1 — Canonical catalog

Status: **completed**

- Frontend and backend catalog reporting now use `.agents/plugins/marketplace.json` and registered plugin source folders.
- Catalog entries are deduplicated by declared `SKILL.md` name.
- `get_skill_catalog` returns a typed object directly, avoiding escaped JSON strings.
- Native MAF execution remains on `.pmcro/skills-staging` through `AgentSkillsProvider`.
- Plugin and skill ordering is stable in the frontend.

## Product direction

The next batches will make PMCR-O a self-referential software-production system:

- Workspaces own software tasks and artifacts.
- Trails own auditable execution evidence.
- Skills provide progressive domain expertise.
- The Colony can audit its own skills, trails, workspaces, and repeated constraints.

## Safety boundary

No existing skills or generated staging files were deleted. Deprecation and cleanup require a separate audit and approval step.

## Validation

The focused MTP test project and full solution build are being rerun after the canonical catalog source correction.

## Catalog correction

The earlier implementation still read the backend catalog from stale `.pmcro/skills-staging` and serialized the tool result as a JSON string. Corrected to:

- read `.agents/plugins/marketplace.json` in `SkillCatalogService`
- deduplicate by declared skill name
- return `SkillCatalogSnapshot` directly from Orchestrator and Harness
- retain `.pmcro/skills-staging` only for native MAF execution

This directly addresses the 135 vs 134 count drift and escaped `get_skill_catalog` output.

## Verification

- `get_skill_catalog` now returns `SkillCatalogSnapshot` directly for both Orchestrator and Harness.
- The backend catalog reader is marketplace-based and aligned with the frontend.
- AppHost processes were stopped before the final build to prevent file-lock warnings.
