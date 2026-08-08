# Declarative Workflows

MAF 1.0 supports defining workflow topology, agent prompts, and edge
conditions in YAML instead of compiled code:

- .NET: `Microsoft.Agents.AI.Workflows.Declarative`
- Python: `agent-framework-declarative`

The runtime parses the YAML and instantiates the same underlying
Executor/Edge graph a hand-written `WorkflowBuilder` call would produce
-- declarative and code-first are two authoring surfaces over one
execution model, not two different engines.

## Why This Matters

Moving graph topology out of compiled code means routing logic and agent
instructions can change without a rebuild/redeploy cycle. For a
catalog-driven system like this repo's marketplace plugins (where
skills and domains are already data-driven via `catalog/skills.json`
and `marketplace.json`), a declarative MAF workflow is the same
philosophy applied one layer down -- the graph itself becomes catalog
data.

## Shape

A declarative workflow YAML generally declares:

1. **Executors** -- id, type (agent-backed or plain function), and its
   typed input/output contract
2. **Edges** -- source executor id, target executor id, and an optional
   condition expression
3. **Entry point** -- which executor receives the workflow's initial
   input
4. **HIL gates** -- which executors require a human approval event
   before their output is allowed to proceed downstream

See `assets/vendor-onboarding.workflow.yaml` for a concrete example
against this repo's marketplace domain, and
`scripts/validate_workflow_yaml.py` for a parser-level sanity check
(does every edge reference an executor id that actually exists, does
every HIL-gated executor have a defined approval path).
