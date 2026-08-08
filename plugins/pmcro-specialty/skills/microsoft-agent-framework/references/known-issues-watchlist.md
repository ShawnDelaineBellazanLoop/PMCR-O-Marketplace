# Known Issues Watchlist -- Verification Status

This file exists specifically to stop an unverified claim from silently
becoming "known fact" just because it's written in a reference doc.
EC-VERIFY-FIRST-001 applies to imported knowledge as much as to a prior
turn's own claims.

## Origin

A separate Gemini session drafted an initial version of this skill's
content and cited four specific GitHub issue numbers on
`microsoft/agent-framework` as "known bugs":

- .NET #7156 -- claimed: session-state update regression, external
  skills fail to persist state back to the provider
- .NET #6786 -- claimed: `RequestPort` input state loss, an Executor's
  `TInput` loses state during an external event loopback while the event
  response itself survives
- Python #7527 -- claimed: `workflow.as_agent()` wrapped in
  `AgentFrameworkAgent` fails to inject `HandoffAgentUserRequest` info
  into `RUN_FINISHED` interrupts inside AG-UI
- Python #6271 -- claimed: related/duplicate of #7527

## What Got Checked Before This Skill Was Written

The architectural claims elsewhere in this skill (Executors/Edges,
Agent Harness, CodeAct+Hyperlight, FIDES, declarative YAML,
Pregel-style checkpointing) were verified against Microsoft's own docs
and current search results, and held up.

The four issue numbers above did **not** get the same treatment.
Issue #7156 was confirmed to exist as an open issue on the repo at
verification time -- but its actual title and body were not read, so
whether it's really about session-state persistence is unconfirmed.
Issues #6786, #7527, and #6271 were not checked at all.

## Rule Going Forward

Do not cite any of the four issue numbers above as fact in a SKILL.md,
reference doc, commit message, or trail file until each has been opened
directly (`https://github.com/microsoft/agent-framework/issues/<n>`) and
its actual title/body/labels confirmed to match the claimed behavior.
If a cycle needs to document a real known-bug caveat for MAF, verify
first and update this file with the confirmed issue number, title, and
a one-line summary in the caller's own words -- not copied from an
unverified prior summary.
