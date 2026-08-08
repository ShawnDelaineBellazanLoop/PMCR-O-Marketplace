# Official MAF + CopilotKit Research

## Key findings

### Microsoft Agent Framework

Official MAF documentation describes Agent Skills as portable packages with progressive disclosure:

1. Advertise names/descriptions.
2. `load_skill` for full instructions.
3. `read_skill_resource` for references/assets.
4. `run_skill_script` for executable scripts.

The official guidance recommends keeping skill instructions concise, using the standard `SKILL.md` layout, and treating scripts as code requiring sandboxing, limits, allow-listing, approvals, and audit logging.

MAF's `AgentSkillsProvider` supports file, inline, class, and MCP skill sources, plus aggregation, deduplication, caching, and filtering.

### CopilotKit

Official CopilotKit documentation recommends a customization ladder:

- prebuilt chat components
- CSS reskinning
- slot-level customization
- headless UI with `useAgent` and `useCopilotKit`

The project should customize the existing CopilotKit chat with slots rather than hide its input and rebuild interaction behavior separately.

### A2UI

For stable PMCR-O surfaces, official A2UI guidance favors fixed schemas: define the catalog and schema once, then let the agent provide data. This is preferable for cycle evidence, skill-load status, HIL status, and trail summaries because it prevents model-generated layout drift.

## Repository mapping

| Official guidance | Repository implementation | Status |
|---|---|---|
| File-based `SKILL.md` provider | `.agents/plugins/marketplace.json` → `.pmcro/skills-staging` → `AgentSkillsProvider` | Implemented |
| Progressive disclosure | Native MAF provider on Orchestrator, Harness, CodeAct | Implemented in source; runtime smoke test required |
| Deduplication | `SkillCatalogService` and frontend catalog dedupe | Implemented |
| Script safety | CodeAct read-only tools and HIL boundary | Partial; add script audit/limits |
| CopilotKit slots | Existing header/mode slot; hidden input still needs UX correction | Partial |
| `useAgent` state | PMCR-O phase state bridge | Implemented |
| Fixed-schema A2UI | Current inline A2UI bridge | Partial; catalog renderer mapping still needs completion |

## Next corrections

1. Use CopilotKit slot composition for the visible conversation surface instead of relying on a hidden input plus a separate custom composer.
2. Expose skill-load status as grounded agent state: advertised, loaded, resource-read, script-requested, approved/rejected.
3. Add fixed-schema A2UI components for cycle summary, skill context, HIL decision, and trail evidence.
4. Add script source review, allow-listing, execution timeouts, and audit records.
5. Validate runtime behavior with fresh Orchestrator/Harness conversations and Playwright.
