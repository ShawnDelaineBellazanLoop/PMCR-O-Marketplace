# Official MAF and CopilotKit Guidance

## Sources

- [Microsoft Agent Skills](https://learn.microsoft.com/en-us/agent-framework/agents/skills)
- [Adding Skills with Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/journey/adding-skills)
- [Agent Skills specification](https://agentskills.io/specification)
- [CopilotKit prebuilt components](https://docs.copilotkit.ai/strands/prebuilt-components)
- [CopilotKit slots](https://docs.showcase.copilotkit.ai/custom-look-and-feel/slots)
- [CopilotKit fixed-schema A2UI](https://docs.copilotkit.ai/agent-spec/generative-ui/a2ui/fixed-schema)
- [Microsoft Agent Framework AgentSkillsProvider source](https://github.com/microsoft/agent-framework/blob/main/dotnet/src/Microsoft.Agents.AI/Skills/AgentSkillsProvider.cs)
- [CopilotKit source](https://github.com/copilotkit/copilotkit)

## MAF Agent Skills requirements

A skill is a directory containing `SKILL.md` with YAML frontmatter. The required metadata is `name` and `description`. The name must be 1–64 characters, lowercase alphanumeric plus hyphens, and must match the parent directory name. Optional resources belong in `references/` and `assets/`; executable code belongs in `scripts/`.

MAF uses progressive disclosure:

1. Advertise skill names and descriptions.
2. Call `load_skill` when the task matches.
3. Call `read_skill_resource` only when a referenced resource is needed.
4. Call `run_skill_script` only when a script is required.

Do not inject every `SKILL.md` body into the initial prompt. The UI catalog may show metadata, but the agent remains responsible for progressive loading.

`AgentSkillsProvider` supports file-based, inline, class-based, and MCP-based sources. Provider composition can use aggregation, deduplication, caching, and filtering. Production providers should keep caching enabled unless development hot reload requires disabling it.

Skill scripts are executable code and require production controls: trusted source review, sandboxing, resource limits, allow-listing, input validation, explicit approval, and audit logging.

## CopilotKit UI guidance

CopilotKit supports a customization ladder:

1. Prebuilt `CopilotChat`, `CopilotSidebar`, or `CopilotPopup`.
2. CSS token and class reskinning.
3. Slot-level recomposition for header, welcome screen, messages, composer, tool calls, and toggle button.
4. Headless composition with `useAgent`, `useCopilotKit`, and tool-rendering hooks.

Use slots before replacing the entire chat surface. Keep the existing CopilotKit runtime and streaming behavior intact while customizing presentation.

`useAgent` is the programmatic AG-UI surface for agent state and control. The visible workbench should use it for grounded run status; do not duplicate backend state with decorative timers.

## A2UI guidance

Use fixed-schema A2UI for known PMCR-O surfaces such as cycle summaries, skill-load status, HIL decisions, and trail evidence. Define the catalog and schema deterministically; let the agent provide data. Do not let the model invent the entire page structure for stable product surfaces.

For a fixed schema, the backend owns the tool/data operations and the frontend registers the matching component catalog. Disable runtime A2UI tool injection when the agent already owns the fixed-schema tool.

## Project application

- Keep `.agents/plugins/marketplace.json` as the registry.
- Materialize into `.pmcro/skills-staging`.
- Use native `AgentSkillsProvider` for Orchestrator, Harness, and CodeAct.
- Use the frontend catalog only for discovery and selection metadata.
- Validate selected skill names against the canonical staged catalog before execution.
- Keep `/api/copilotkit` as the browser boundary and `/agui`/`/agui/harness` as server-side AG-UI endpoints.
- Prefer structured workbench sections and visible request state over a hidden chat-only submission path.
